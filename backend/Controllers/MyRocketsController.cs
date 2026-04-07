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
[Route("api/v1/my/rockets")]
public sealed class MyRocketsController : ControllerBase
{
    private static readonly HashSet<string> AllowedConditions =
    new(["New", "Good", "Fair", "Retired"], StringComparer.OrdinalIgnoreCase);

    private readonly IMongoCollection<Rocket> _rockets;
    private readonly IMongoCollection<UserRocket> _userRockets;

    public MyRocketsController(MongoDbContext context)
    {
        _rockets = context.GetCollection<Rocket>();
        _userRockets = context.GetCollection<UserRocket>();
    }

    [HttpPost]
    public async Task<IActionResult> AddRocketFromCatalog(
        [FromBody] AddRocketFromCatalogRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.RocketId))
        {
            return BadRequest(new ApiErrorResponse("Rocket id is required.", "RocketIdRequired"));
        }

        if (!ObjectId.TryParse(request.RocketId, out var rocketId))
        {
            return BadRequest(new ApiErrorResponse("Rocket id is invalid.", "InvalidRocketId"));
        }

        var rocketExists = await _rockets
            .Find(Builders<Rocket>.Filter.And(
                Builders<Rocket>.Filter.Eq(rocket => rocket.Id, rocketId),
                Builders<Rocket>.Filter.Eq(rocket => rocket.IsActive, true)))
            .AnyAsync(cancellationToken);

        if (!rocketExists)
        {
            return NotFound(new ApiErrorResponse("Rocket not found.", "RocketNotFound"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var existing = await _userRockets
            .Find(Builders<UserRocket>.Filter.And(
                Builders<UserRocket>.Filter.Eq(rocket => rocket.UserId, userId),
                Builders<UserRocket>.Filter.Eq(rocket => rocket.RocketId, rocketId)))
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            return Ok(new AddRocketFromCatalogResponse(
                existing.Id.ToString(),
                existing.RocketId.ToString(),
                false));
        }

        var now = DateTime.UtcNow;
        var userRocket = new UserRocket
        {
            UserId = userId,
            RocketId = rocketId,
            Condition = "New",
            CreatedAt = now,
            UpdatedAt = now
        };

        await _userRockets.InsertOneAsync(userRocket, cancellationToken: cancellationToken);

        return Ok(new AddRocketFromCatalogResponse(
            userRocket.Id.ToString(),
            userRocket.RocketId.ToString(),
            true));
    }

    [HttpGet]
    public async Task<IActionResult> GetMyRockets(CancellationToken cancellationToken = default)
    {
        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var filter = Builders<UserRocket>.Filter.Eq(rocket => rocket.UserId, userId);

        var userRockets = await _userRockets
            .Find(filter)
            .SortByDescending(rocket => rocket.UpdatedAt)
            .ToListAsync(cancellationToken);

        var rocketIds = userRockets
            .Select(rocket => rocket.RocketId)
            .Distinct()
            .ToList();

        var rockets = rocketIds.Count == 0
            ? []
            : await _rockets
                .Find(Builders<Rocket>.Filter.In(rocket => rocket.Id, rocketIds))
                .ToListAsync(cancellationToken);

        var rocketById = rockets.ToDictionary(rocket => rocket.Id, rocket => rocket);

        var items = userRockets
            .Select(userRocket =>
            {
                rocketById.TryGetValue(userRocket.RocketId, out var rocket);
                return MapUserRocket(userRocket, rocket);
            })
            .ToArray();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMyRocketById(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var userRocketId))
        {
            return BadRequest(new ApiErrorResponse("User rocket id is invalid.", "InvalidUserRocketId"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var userRocket = await _userRockets
            .Find(Builders<UserRocket>.Filter.And(
                Builders<UserRocket>.Filter.Eq(rocket => rocket.Id, userRocketId),
                Builders<UserRocket>.Filter.Eq(rocket => rocket.UserId, userId)))
            .FirstOrDefaultAsync(cancellationToken);

        if (userRocket is null)
        {
            return NotFound(new ApiErrorResponse("User rocket not found.", "UserRocketNotFound"));
        }

        var rocket = await _rockets
            .Find(Builders<Rocket>.Filter.Eq(item => item.Id, userRocket.RocketId))
            .FirstOrDefaultAsync(cancellationToken);

        return Ok(MapUserRocket(userRocket, rocket));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMyRocket(
        string id,
        [FromBody] UpdateUserRocketRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest(new ApiErrorResponse("Request body is required.", "RequestRequired"));
        }

        if (!ObjectId.TryParse(id, out var userRocketId))
        {
            return BadRequest(new ApiErrorResponse("User rocket id is invalid.", "InvalidUserRocketId"));
        }

        var normalizedCondition = request.Condition?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedCondition) || !AllowedConditions.Contains(normalizedCondition))
        {
            return BadRequest(new ApiErrorResponse("Condition is invalid.", "InvalidCondition"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var userRocket = await _userRockets
            .Find(Builders<UserRocket>.Filter.And(
                Builders<UserRocket>.Filter.Eq(rocket => rocket.Id, userRocketId),
                Builders<UserRocket>.Filter.Eq(rocket => rocket.UserId, userId)))
            .FirstOrDefaultAsync(cancellationToken);

        if (userRocket is null)
        {
            return NotFound(new ApiErrorResponse("User rocket not found.", "UserRocketNotFound"));
        }

        userRocket.Nickname = string.IsNullOrWhiteSpace(request.Nickname) ? null : request.Nickname.Trim();
        userRocket.BuildDate = request.BuildDate;
        userRocket.Condition = normalizedCondition;
        userRocket.BuildNotes = string.IsNullOrWhiteSpace(request.BuildNotes) ? null : request.BuildNotes.Trim();
        userRocket.PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl) ? null : request.PhotoUrl.Trim();
        userRocket.UpdatedAt = DateTime.UtcNow;

        await _userRockets.ReplaceOneAsync(
            Builders<UserRocket>.Filter.Eq(rocket => rocket.Id, userRocket.Id),
            userRocket,
            cancellationToken: cancellationToken);

        var rocket = await _rockets
            .Find(Builders<Rocket>.Filter.Eq(item => item.Id, userRocket.RocketId))
            .FirstOrDefaultAsync(cancellationToken);

        return Ok(MapUserRocket(userRocket, rocket));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMyRocket(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var userRocketId))
        {
            return BadRequest(new ApiErrorResponse("User rocket id is invalid.", "InvalidUserRocketId"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var deleted = await _userRockets.DeleteOneAsync(
            Builders<UserRocket>.Filter.And(
                Builders<UserRocket>.Filter.Eq(rocket => rocket.Id, userRocketId),
                Builders<UserRocket>.Filter.Eq(rocket => rocket.UserId, userId)),
            cancellationToken);

        if (deleted.DeletedCount == 0)
        {
            return NotFound(new ApiErrorResponse("User rocket not found.", "UserRocketNotFound"));
        }

        return NoContent();
    }

    [HttpPost("{id}/photo")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadMyRocketPhoto(
        string id,
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var userRocketId))
        {
            return BadRequest(new ApiErrorResponse("User rocket id is invalid.", "InvalidUserRocketId"));
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new ApiErrorResponse("Photo file is required.", "PhotoFileRequired"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var userRocket = await _userRockets
            .Find(Builders<UserRocket>.Filter.And(
                Builders<UserRocket>.Filter.Eq(rocket => rocket.Id, userRocketId),
                Builders<UserRocket>.Filter.Eq(rocket => rocket.UserId, userId)))
            .FirstOrDefaultAsync(cancellationToken);

        if (userRocket is null)
        {
            return NotFound(new ApiErrorResponse("User rocket not found.", "UserRocketNotFound"));
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".jpg";
        }

        var uploadsRoot = Path.Combine(AppContext.BaseDirectory, "Uploads", "user-rockets", userId.ToString());
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{userRocket.Id}-{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        var fullPath = Path.Combine(uploadsRoot, fileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        userRocket.PhotoUrl = fullPath;
        userRocket.UpdatedAt = DateTime.UtcNow;

        await _userRockets.ReplaceOneAsync(
            Builders<UserRocket>.Filter.Eq(rocket => rocket.Id, userRocket.Id),
            userRocket,
            cancellationToken: cancellationToken);

        return Ok(new UploadUserRocketPhotoResponse(userRocket.Id.ToString(), userRocket.PhotoUrl));
    }

    private static UserRocketItemDto MapUserRocket(UserRocket userRocket, Rocket? rocket)
    {
        return new UserRocketItemDto(
            userRocket.Id.ToString(),
            userRocket.RocketId.ToString(),
            rocket?.Manufacturer,
            rocket?.Sku,
            rocket?.Name,
            rocket?.ThumbnailUrl,
            userRocket.Nickname,
            userRocket.BuildDate,
            userRocket.Condition,
            userRocket.BuildNotes,
            userRocket.PhotoUrl,
            userRocket.CreatedAt,
            userRocket.UpdatedAt);
    }

    public sealed record AddRocketFromCatalogRequest(string RocketId);

    public sealed record AddRocketFromCatalogResponse(string UserRocketId, string RocketId, bool Added);

    public sealed record UpdateUserRocketRequest(
        string? Nickname,
        DateTime? BuildDate,
        string Condition,
        string? BuildNotes,
        string? PhotoUrl);

    public sealed record UploadUserRocketPhotoResponse(string UserRocketId, string? PhotoUrl);

    public sealed record UserRocketItemDto(
        string Id,
        string RocketId,
        string? Manufacturer,
        string? Sku,
        string? Name,
        string? ThumbnailUrl,
        string? Nickname,
        DateTime? BuildDate,
        string Condition,
        string? BuildNotes,
        string? PhotoUrl,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}