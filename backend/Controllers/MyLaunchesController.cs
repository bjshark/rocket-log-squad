using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using RocketLog.Api.Data;
using RocketLog.Api.Models.Common;
using RocketLog.Api.Models.Domain;

namespace RocketLog.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/my/launches")]
public sealed class MyLaunchesController : ControllerBase
{
    private static readonly HashSet<string> AllowedOutcomes =
        new(["Success", "Partial", "Failure", "No Launch"], StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> AllowedWeatherSources =
        new(["api", "manual"], StringComparer.OrdinalIgnoreCase);

    private readonly IMongoCollection<Launch> _launches;
    private readonly IMongoCollection<UserRocket> _userRockets;
    private readonly IMongoCollection<Rocket> _rockets;
    private readonly IMongoCollection<Engine> _engines;

    public MyLaunchesController(MongoDbContext context)
    {
        _launches = context.GetCollection<Launch>();
        _userRockets = context.GetCollection<UserRocket>();
        _rockets = context.GetCollection<Rocket>();
        _engines = context.GetCollection<Engine>();
    }

    [HttpGet]
    public async Task<IActionResult> GetMyLaunches(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var validatedPage = page < 1 ? 1 : page;
        var validatedPageSize = Math.Clamp(pageSize, 1, 100);

        var filter = Builders<Launch>.Filter.Eq(item => item.UserId, userId);

        var total = await _launches.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await _launches
            .Find(filter)
            .SortByDescending(item => item.LaunchDate)
            .Skip((validatedPage - 1) * validatedPageSize)
            .Limit(validatedPageSize)
            .ToListAsync(cancellationToken);

        var launchDtos = await MapLaunchesAsync(items, cancellationToken);

        return Ok(new PagedResponse<LaunchListItemDto>(launchDtos, total, validatedPage, validatedPageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMyLaunchById(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var launchId))
        {
            return BadRequest(new ApiErrorResponse("Launch id is invalid.", "InvalidLaunchId"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var launch = await _launches
            .Find(Builders<Launch>.Filter.And(
                Builders<Launch>.Filter.Eq(item => item.Id, launchId),
                Builders<Launch>.Filter.Eq(item => item.UserId, userId)))
            .FirstOrDefaultAsync(cancellationToken);

        if (launch is null)
        {
            return NotFound(new ApiErrorResponse("Launch not found.", "LaunchNotFound"));
        }

        var detail = await MapLaunchDetailAsync(launch, cancellationToken);
        return Ok(detail);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLaunch(
        [FromBody] CreateLaunchRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest(new ApiErrorResponse("Request body is required.", "RequestRequired"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var validationError = await ValidateLaunchRequestAsync(request, userId, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var now = DateTime.UtcNow;

        var launch = new Launch
        {
            UserId = userId,
            UserRocketId = ObjectId.Parse(request.UserRocketId),
            EngineId = ObjectId.Parse(request.EngineId),
            LaunchDate = request.LaunchDate,
            Location = new LaunchLocation
            {
                Name = NormalizeNullable(request.Location.Name),
                Lat = request.Location.Lat,
                Lng = request.Location.Lng
            },
            Weather = new LaunchWeather
            {
                Source = request.Weather.Source.Trim().ToLowerInvariant(),
                TemperatureF = request.Weather.TemperatureF,
                WindSpeedMph = request.Weather.WindSpeedMph,
                WindDirection = request.Weather.WindDirection.Trim(),
                Humidity = request.Weather.Humidity,
                Conditions = request.Weather.Conditions.Trim(),
                VisibilityMi = request.Weather.VisibilityMi
            },
            Outcome = NormalizeOutcome(request.Outcome),
            AltitudeFt = request.AltitudeFt,
            Notes = NormalizeNullable(request.Notes),
            PhotoUrl = NormalizeNullable(request.PhotoUrl),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _launches.InsertOneAsync(launch, cancellationToken: cancellationToken);

        var created = await MapLaunchDetailAsync(launch, cancellationToken);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLaunch(
        string id,
        [FromBody] UpdateLaunchRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest(new ApiErrorResponse("Request body is required.", "RequestRequired"));
        }

        if (!ObjectId.TryParse(id, out var launchId))
        {
            return BadRequest(new ApiErrorResponse("Launch id is invalid.", "InvalidLaunchId"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var existing = await _launches
            .Find(Builders<Launch>.Filter.And(
                Builders<Launch>.Filter.Eq(item => item.Id, launchId),
                Builders<Launch>.Filter.Eq(item => item.UserId, userId)))
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            return NotFound(new ApiErrorResponse("Launch not found.", "LaunchNotFound"));
        }

        var validationError = await ValidateLaunchRequestAsync(request, userId, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        existing.UserRocketId = ObjectId.Parse(request.UserRocketId);
        existing.EngineId = ObjectId.Parse(request.EngineId);
        existing.LaunchDate = request.LaunchDate;
        existing.Location = new LaunchLocation
        {
            Name = NormalizeNullable(request.Location.Name),
            Lat = request.Location.Lat,
            Lng = request.Location.Lng
        };
        existing.Weather = new LaunchWeather
        {
            Source = request.Weather.Source.Trim().ToLowerInvariant(),
            TemperatureF = request.Weather.TemperatureF,
            WindSpeedMph = request.Weather.WindSpeedMph,
            WindDirection = request.Weather.WindDirection.Trim(),
            Humidity = request.Weather.Humidity,
            Conditions = request.Weather.Conditions.Trim(),
            VisibilityMi = request.Weather.VisibilityMi
        };
        existing.Outcome = NormalizeOutcome(request.Outcome);
        existing.AltitudeFt = request.AltitudeFt;
        existing.Notes = NormalizeNullable(request.Notes);
        existing.PhotoUrl = NormalizeNullable(request.PhotoUrl);
        existing.UpdatedAt = DateTime.UtcNow;

        await _launches.ReplaceOneAsync(
            Builders<Launch>.Filter.Eq(item => item.Id, existing.Id),
            existing,
            cancellationToken: cancellationToken);

        var updated = await MapLaunchDetailAsync(existing, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLaunch(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var launchId))
        {
            return BadRequest(new ApiErrorResponse("Launch id is invalid.", "InvalidLaunchId"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var deleted = await _launches.DeleteOneAsync(
            Builders<Launch>.Filter.And(
                Builders<Launch>.Filter.Eq(item => item.Id, launchId),
                Builders<Launch>.Filter.Eq(item => item.UserId, userId)),
            cancellationToken);

        if (deleted.DeletedCount == 0)
        {
            return NotFound(new ApiErrorResponse("Launch not found.", "LaunchNotFound"));
        }

        return NoContent();
    }

    [HttpPost("{id}/photo")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadLaunchPhoto(
        string id,
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var launchId))
        {
            return BadRequest(new ApiErrorResponse("Launch id is invalid.", "InvalidLaunchId"));
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new ApiErrorResponse("Photo file is required.", "PhotoFileRequired"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var launch = await _launches
            .Find(Builders<Launch>.Filter.And(
                Builders<Launch>.Filter.Eq(item => item.Id, launchId),
                Builders<Launch>.Filter.Eq(item => item.UserId, userId)))
            .FirstOrDefaultAsync(cancellationToken);

        if (launch is null)
        {
            return NotFound(new ApiErrorResponse("Launch not found.", "LaunchNotFound"));
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".jpg";
        }

        var uploadsRoot = Path.Combine(AppContext.BaseDirectory, "Uploads", "launches", userId.ToString());
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{launch.Id}-{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        var fullPath = Path.Combine(uploadsRoot, fileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        launch.PhotoUrl = fullPath;
        launch.UpdatedAt = DateTime.UtcNow;

        await _launches.ReplaceOneAsync(
            Builders<Launch>.Filter.Eq(item => item.Id, launch.Id),
            launch,
            cancellationToken: cancellationToken);

        return Ok(new UploadLaunchPhotoResponse(launch.Id.ToString(), launch.PhotoUrl));
    }

    private async Task<IActionResult?> ValidateLaunchRequestAsync(
        LaunchMutationRequest request,
        ObjectId userId,
        CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(request.UserRocketId, out var userRocketId))
        {
            return BadRequest(new ApiErrorResponse("User rocket id is invalid.", "InvalidUserRocketId"));
        }

        if (!ObjectId.TryParse(request.EngineId, out var engineId))
        {
            return BadRequest(new ApiErrorResponse("Engine id is invalid.", "InvalidEngineId"));
        }

        if (request.LaunchDate == default)
        {
            return BadRequest(new ApiErrorResponse("Launch date is required.", "LaunchDateRequired"));
        }

        if (!AllowedOutcomes.Contains(request.Outcome ?? string.Empty))
        {
            return BadRequest(new ApiErrorResponse("Outcome is invalid.", "InvalidOutcome"));
        }

        if (request.Location.Lat < -90 || request.Location.Lat > 90)
        {
            return BadRequest(new ApiErrorResponse("Latitude must be between -90 and 90.", "InvalidLatitude"));
        }

        if (request.Location.Lng < -180 || request.Location.Lng > 180)
        {
            return BadRequest(new ApiErrorResponse("Longitude must be between -180 and 180.", "InvalidLongitude"));
        }

        if (!AllowedWeatherSources.Contains(request.Weather.Source ?? string.Empty))
        {
            return BadRequest(new ApiErrorResponse("Weather source is invalid.", "InvalidWeatherSource"));
        }

        if (string.IsNullOrWhiteSpace(request.Weather.WindDirection))
        {
            return BadRequest(new ApiErrorResponse("Wind direction is required.", "WindDirectionRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.Weather.Conditions))
        {
            return BadRequest(new ApiErrorResponse("Weather conditions are required.", "WeatherConditionsRequired"));
        }

        if (request.Weather.Humidity < 0 || request.Weather.Humidity > 100)
        {
            return BadRequest(new ApiErrorResponse("Humidity must be between 0 and 100.", "InvalidHumidity"));
        }

        if (request.Weather.WindSpeedMph < 0)
        {
            return BadRequest(new ApiErrorResponse("Wind speed cannot be negative.", "InvalidWindSpeed"));
        }

        if (request.AltitudeFt is < 0)
        {
            return BadRequest(new ApiErrorResponse("Altitude cannot be negative.", "InvalidAltitude"));
        }

        var userRocketExists = await _userRockets
            .Find(Builders<UserRocket>.Filter.And(
                Builders<UserRocket>.Filter.Eq(item => item.Id, userRocketId),
                Builders<UserRocket>.Filter.Eq(item => item.UserId, userId)))
            .AnyAsync(cancellationToken);

        if (!userRocketExists)
        {
            return NotFound(new ApiErrorResponse("User rocket not found.", "UserRocketNotFound"));
        }

        var engineExists = await _engines
            .Find(Builders<Engine>.Filter.And(
                Builders<Engine>.Filter.Eq(item => item.Id, engineId),
                Builders<Engine>.Filter.Eq(item => item.IsActive, true)))
            .AnyAsync(cancellationToken);

        if (!engineExists)
        {
            return NotFound(new ApiErrorResponse("Engine not found.", "EngineNotFound"));
        }

        return null;
    }

    private async Task<IReadOnlyList<LaunchListItemDto>> MapLaunchesAsync(
        IReadOnlyList<Launch> launches,
        CancellationToken cancellationToken)
    {
        var rocketIds = launches.Select(item => item.UserRocketId).Distinct().ToList();
        var engineIds = launches.Select(item => item.EngineId).Distinct().ToList();

        var userRockets = rocketIds.Count == 0
            ? []
            : await _userRockets
                .Find(Builders<UserRocket>.Filter.In(item => item.Id, rocketIds))
                .ToListAsync(cancellationToken);

        var rockets = userRockets.Count == 0
            ? []
            : await _rockets
                .Find(Builders<Rocket>.Filter.In(item => item.Id, userRockets.Select(rocket => rocket.RocketId)))
                .ToListAsync(cancellationToken);

        var engines = engineIds.Count == 0
            ? []
            : await _engines
                .Find(Builders<Engine>.Filter.In(item => item.Id, engineIds))
                .ToListAsync(cancellationToken);

        var userRocketById = userRockets.ToDictionary(item => item.Id, item => item);
        var rocketById = rockets.ToDictionary(item => item.Id, item => item);
        var engineById = engines.ToDictionary(item => item.Id, item => item);

        return launches.Select(launch =>
        {
            userRocketById.TryGetValue(launch.UserRocketId, out var userRocket);
            Rocket? rocket = null;
            if (userRocket is not null)
            {
                rocketById.TryGetValue(userRocket.RocketId, out rocket);
            }

            engineById.TryGetValue(launch.EngineId, out var engine);

            return new LaunchListItemDto(
                launch.Id.ToString(),
                launch.LaunchDate,
                launch.Outcome,
                launch.Location.Name,
                launch.Location.Lat,
                launch.Location.Lng,
                launch.UserRocketId.ToString(),
                userRocket?.Nickname,
                rocket?.Name,
                launch.EngineId.ToString(),
                engine?.Designation,
                launch.PhotoUrl,
                launch.UpdatedAt);
        }).ToArray();
    }

    private async Task<LaunchDetailDto> MapLaunchDetailAsync(Launch launch, CancellationToken cancellationToken)
    {
        var userRocket = await _userRockets
            .Find(Builders<UserRocket>.Filter.Eq(item => item.Id, launch.UserRocketId))
            .FirstOrDefaultAsync(cancellationToken);

        Rocket? rocket = null;
        if (userRocket is not null)
        {
            rocket = await _rockets
                .Find(Builders<Rocket>.Filter.Eq(item => item.Id, userRocket.RocketId))
                .FirstOrDefaultAsync(cancellationToken);
        }

        var engine = await _engines
            .Find(Builders<Engine>.Filter.Eq(item => item.Id, launch.EngineId))
            .FirstOrDefaultAsync(cancellationToken);

        return new LaunchDetailDto(
            launch.Id.ToString(),
            launch.UserRocketId.ToString(),
            userRocket?.Nickname,
            rocket?.Name,
            launch.EngineId.ToString(),
            engine?.Designation,
            launch.LaunchDate,
            new LaunchLocationDto(
                launch.Location.Name,
                launch.Location.Lat,
                launch.Location.Lng),
            new LaunchWeatherDto(
                launch.Weather.Source,
                launch.Weather.TemperatureF,
                launch.Weather.WindSpeedMph,
                launch.Weather.WindDirection,
                launch.Weather.Humidity,
                launch.Weather.Conditions,
                launch.Weather.VisibilityMi),
            launch.Outcome,
            launch.AltitudeFt,
            launch.Notes,
            launch.PhotoUrl,
            launch.CreatedAt,
            launch.UpdatedAt);
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeOutcome(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;

        return normalized switch
        {
            "success" => "Success",
            "partial" => "Partial",
            "failure" => "Failure",
            "no launch" => "No Launch",
            _ => "Success"
        };
    }

    public interface LaunchMutationRequest
    {
        string UserRocketId { get; }

        string EngineId { get; }

        DateTime LaunchDate { get; }

        LaunchLocationRequest Location { get; }

        LaunchWeatherRequest Weather { get; }

        string Outcome { get; }

        double? AltitudeFt { get; }

        string? Notes { get; }

        string? PhotoUrl { get; }
    }

    public sealed record CreateLaunchRequest(
        string UserRocketId,
        string EngineId,
        DateTime LaunchDate,
        LaunchLocationRequest Location,
        LaunchWeatherRequest Weather,
        string Outcome,
        double? AltitudeFt,
        string? Notes,
        string? PhotoUrl) : LaunchMutationRequest;

    public sealed record UpdateLaunchRequest(
        string UserRocketId,
        string EngineId,
        DateTime LaunchDate,
        LaunchLocationRequest Location,
        LaunchWeatherRequest Weather,
        string Outcome,
        double? AltitudeFt,
        string? Notes,
        string? PhotoUrl) : LaunchMutationRequest;

    public sealed record LaunchLocationRequest(string? Name, double Lat, double Lng);

    public sealed record LaunchWeatherRequest(
        string Source,
        double TemperatureF,
        double WindSpeedMph,
        string WindDirection,
        double Humidity,
        string Conditions,
        double? VisibilityMi);

    public sealed record UploadLaunchPhotoResponse(string LaunchId, string? PhotoUrl);

    public sealed record LaunchListItemDto(
        string Id,
        DateTime LaunchDate,
        string Outcome,
        string? LocationName,
        double Lat,
        double Lng,
        string UserRocketId,
        string? RocketNickname,
        string? RocketName,
        string EngineId,
        string? EngineDesignation,
        string? PhotoUrl,
        DateTime UpdatedAt);

    public sealed record LaunchDetailDto(
        string Id,
        string UserRocketId,
        string? RocketNickname,
        string? RocketName,
        string EngineId,
        string? EngineDesignation,
        DateTime LaunchDate,
        LaunchLocationDto Location,
        LaunchWeatherDto Weather,
        string Outcome,
        double? AltitudeFt,
        string? Notes,
        string? PhotoUrl,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public sealed record LaunchLocationDto(string? Name, double Lat, double Lng);

    public sealed record LaunchWeatherDto(
        string Source,
        double TemperatureF,
        double WindSpeedMph,
        string WindDirection,
        double Humidity,
        string Conditions,
        double? VisibilityMi);
}
