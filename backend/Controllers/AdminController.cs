using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using RocketLog.Api.Data;
using RocketLog.Api.Models.Common;
using RocketLog.Api.Models.Domain;

namespace RocketLog.Api.Controllers;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/v1/admin")]
public sealed class AdminController : ControllerBase
{
    private static readonly Regex UploadScopeRegex = new("^[a-z0-9_-]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IMongoCollection<Rocket> _rockets;
    private readonly IMongoCollection<Engine> _engines;

    public AdminController(MongoDbContext context)
    {
        _rockets = context.GetCollection<Rocket>();
        _engines = context.GetCollection<Engine>();
    }

    [HttpGet("rockets")]
    public async Task<IActionResult> GetAdminRockets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var validatedPage = page < 1 ? 1 : page;
        var validatedPageSize = Math.Clamp(pageSize, 1, 100);

        var filter = FilterDefinition<Rocket>.Empty;
        var total = await _rockets.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await _rockets
            .Find(filter)
            .SortBy(rocket => rocket.Manufacturer)
            .ThenBy(rocket => rocket.Name)
            .Skip((validatedPage - 1) * validatedPageSize)
            .Limit(validatedPageSize)
            .ToListAsync(cancellationToken);

        return Ok(new PagedResponse<AdminRocketDto>(
            items.Select(MapAdminRocket).ToArray(),
            total,
            validatedPage,
            validatedPageSize));
    }

    [HttpGet("rockets/{id}")]
    public async Task<IActionResult> GetAdminRocketById(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var rocketId))
        {
            return BadRequest(new ApiErrorResponse("Rocket id is invalid.", "InvalidRocketId"));
        }

        var rocket = await _rockets
            .Find(Builders<Rocket>.Filter.Eq(item => item.Id, rocketId))
            .FirstOrDefaultAsync(cancellationToken);

        if (rocket is null)
        {
            return NotFound(new ApiErrorResponse("Rocket not found.", "RocketNotFound"));
        }

        return Ok(MapAdminRocket(rocket));
    }

    [HttpPost("rockets")]
    public async Task<IActionResult> CreateRocket(
        [FromBody] AdminUpsertRocketRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest(new ApiErrorResponse("Request body is required.", "RequestRequired"));
        }

        var validationError = ValidateRocketRequest(request);
        if (validationError is not null)
        {
            return validationError;
        }

        var duplicateExists = await _rockets
            .Find(Builders<Rocket>.Filter.And(
                Builders<Rocket>.Filter.Eq(item => item.Manufacturer, request.Manufacturer.Trim()),
                Builders<Rocket>.Filter.Eq(item => item.Sku, request.Sku.Trim())))
            .AnyAsync(cancellationToken);

        if (duplicateExists)
        {
            return Conflict(new ApiErrorResponse("A rocket with the same manufacturer and SKU already exists.", "RocketAlreadyExists"));
        }

        var now = DateTime.UtcNow;
        var rocket = new Rocket
        {
            Manufacturer = request.Manufacturer.Trim(),
            Sku = request.Sku.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            SkillLevel = request.SkillLevel.Trim(),
            RecommendedEngines = NormalizeStringList(request.RecommendedEngines),
            DiameterMm = request.DiameterMm,
            LengthMm = request.LengthMm,
            WeightG = request.WeightG,
            FinMaterial = request.FinMaterial.Trim(),
            NoseCone = request.NoseCone.Trim(),
            RecoverySystem = request.RecoverySystem.Trim(),
            ThumbnailUrl = request.ThumbnailUrl.Trim(),
            ImageUrls = NormalizeStringList(request.ImageUrls),
            ProductUrl = NormalizeNullable(request.ProductUrl),
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _rockets.InsertOneAsync(rocket, cancellationToken: cancellationToken);
        return Ok(MapAdminRocket(rocket));
    }

    [HttpPut("rockets/{id}")]
    public async Task<IActionResult> UpdateRocket(
        string id,
        [FromBody] AdminUpsertRocketRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest(new ApiErrorResponse("Request body is required.", "RequestRequired"));
        }

        if (!ObjectId.TryParse(id, out var rocketId))
        {
            return BadRequest(new ApiErrorResponse("Rocket id is invalid.", "InvalidRocketId"));
        }

        var validationError = ValidateRocketRequest(request);
        if (validationError is not null)
        {
            return validationError;
        }

        var existing = await _rockets
            .Find(Builders<Rocket>.Filter.Eq(item => item.Id, rocketId))
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            return NotFound(new ApiErrorResponse("Rocket not found.", "RocketNotFound"));
        }

        var duplicateExists = await _rockets
            .Find(Builders<Rocket>.Filter.And(
                Builders<Rocket>.Filter.Ne(item => item.Id, rocketId),
                Builders<Rocket>.Filter.Eq(item => item.Manufacturer, request.Manufacturer.Trim()),
                Builders<Rocket>.Filter.Eq(item => item.Sku, request.Sku.Trim())))
            .AnyAsync(cancellationToken);

        if (duplicateExists)
        {
            return Conflict(new ApiErrorResponse("A rocket with the same manufacturer and SKU already exists.", "RocketAlreadyExists"));
        }

        existing.Manufacturer = request.Manufacturer.Trim();
        existing.Sku = request.Sku.Trim();
        existing.Name = request.Name.Trim();
        existing.Description = request.Description.Trim();
        existing.SkillLevel = request.SkillLevel.Trim();
        existing.RecommendedEngines = NormalizeStringList(request.RecommendedEngines);
        existing.DiameterMm = request.DiameterMm;
        existing.LengthMm = request.LengthMm;
        existing.WeightG = request.WeightG;
        existing.FinMaterial = request.FinMaterial.Trim();
        existing.NoseCone = request.NoseCone.Trim();
        existing.RecoverySystem = request.RecoverySystem.Trim();
        existing.ThumbnailUrl = request.ThumbnailUrl.Trim();
        existing.ImageUrls = NormalizeStringList(request.ImageUrls);
        existing.ProductUrl = NormalizeNullable(request.ProductUrl);
        existing.IsActive = request.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _rockets.ReplaceOneAsync(
            Builders<Rocket>.Filter.Eq(item => item.Id, existing.Id),
            existing,
            cancellationToken: cancellationToken);

        return Ok(MapAdminRocket(existing));
    }

    [HttpDelete("rockets/{id}")]
    public async Task<IActionResult> DeleteRocket(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var rocketId))
        {
            return BadRequest(new ApiErrorResponse("Rocket id is invalid.", "InvalidRocketId"));
        }

        var update = Builders<Rocket>.Update
            .Set(item => item.IsActive, false)
            .Set(item => item.UpdatedAt, DateTime.UtcNow);

        var result = await _rockets.UpdateOneAsync(
            Builders<Rocket>.Filter.Eq(item => item.Id, rocketId),
            update,
            cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            return NotFound(new ApiErrorResponse("Rocket not found.", "RocketNotFound"));
        }

        return NoContent();
    }

    [HttpGet("engines")]
    public async Task<IActionResult> GetAdminEngines(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var validatedPage = page < 1 ? 1 : page;
        var validatedPageSize = Math.Clamp(pageSize, 1, 100);

        var filter = FilterDefinition<Engine>.Empty;
        var total = await _engines.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await _engines
            .Find(filter)
            .SortBy(engine => engine.Manufacturer)
            .ThenBy(engine => engine.Designation)
            .Skip((validatedPage - 1) * validatedPageSize)
            .Limit(validatedPageSize)
            .ToListAsync(cancellationToken);

        return Ok(new PagedResponse<AdminEngineDto>(
            items.Select(MapAdminEngine).ToArray(),
            total,
            validatedPage,
            validatedPageSize));
    }

    [HttpGet("engines/{id}")]
    public async Task<IActionResult> GetAdminEngineById(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var engineId))
        {
            return BadRequest(new ApiErrorResponse("Engine id is invalid.", "InvalidEngineId"));
        }

        var engine = await _engines
            .Find(Builders<Engine>.Filter.Eq(item => item.Id, engineId))
            .FirstOrDefaultAsync(cancellationToken);

        if (engine is null)
        {
            return NotFound(new ApiErrorResponse("Engine not found.", "EngineNotFound"));
        }

        return Ok(MapAdminEngine(engine));
    }

    [HttpPost("engines")]
    public async Task<IActionResult> CreateEngine(
        [FromBody] AdminUpsertEngineRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest(new ApiErrorResponse("Request body is required.", "RequestRequired"));
        }

        var validationError = ValidateEngineRequest(request);
        if (validationError is not null)
        {
            return validationError;
        }

        var duplicateExists = await _engines
            .Find(Builders<Engine>.Filter.And(
                Builders<Engine>.Filter.Eq(item => item.Manufacturer, request.Manufacturer.Trim()),
                Builders<Engine>.Filter.Eq(item => item.Designation, request.Designation.Trim())))
            .AnyAsync(cancellationToken);

        if (duplicateExists)
        {
            return Conflict(new ApiErrorResponse("An engine with the same manufacturer and designation already exists.", "EngineAlreadyExists"));
        }

        var now = DateTime.UtcNow;
        var engine = new Engine
        {
            Manufacturer = request.Manufacturer.Trim(),
            Designation = request.Designation.Trim(),
            ImpulseClass = request.ImpulseClass.Trim().ToUpperInvariant(),
            TotalImpulseNs = request.TotalImpulseNs,
            AverageThrustN = request.AverageThrustN,
            DelayS = request.DelayS,
            DiameterMm = request.DiameterMm,
            LengthMm = request.LengthMm,
            PropellantWeightG = request.PropellantWeightG,
            TotalWeightG = request.TotalWeightG,
            CaseType = request.CaseType.Trim(),
            PropellantType = request.PropellantType.Trim(),
            ThumbnailUrl = request.ThumbnailUrl.Trim(),
            ImageUrls = NormalizeStringList(request.ImageUrls),
            CertificationBody = NormalizeNullable(request.CertificationBody),
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _engines.InsertOneAsync(engine, cancellationToken: cancellationToken);
        return Ok(MapAdminEngine(engine));
    }

    [HttpPut("engines/{id}")]
    public async Task<IActionResult> UpdateEngine(
        string id,
        [FromBody] AdminUpsertEngineRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest(new ApiErrorResponse("Request body is required.", "RequestRequired"));
        }

        if (!ObjectId.TryParse(id, out var engineId))
        {
            return BadRequest(new ApiErrorResponse("Engine id is invalid.", "InvalidEngineId"));
        }

        var validationError = ValidateEngineRequest(request);
        if (validationError is not null)
        {
            return validationError;
        }

        var existing = await _engines
            .Find(Builders<Engine>.Filter.Eq(item => item.Id, engineId))
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            return NotFound(new ApiErrorResponse("Engine not found.", "EngineNotFound"));
        }

        var duplicateExists = await _engines
            .Find(Builders<Engine>.Filter.And(
                Builders<Engine>.Filter.Ne(item => item.Id, engineId),
                Builders<Engine>.Filter.Eq(item => item.Manufacturer, request.Manufacturer.Trim()),
                Builders<Engine>.Filter.Eq(item => item.Designation, request.Designation.Trim())))
            .AnyAsync(cancellationToken);

        if (duplicateExists)
        {
            return Conflict(new ApiErrorResponse("An engine with the same manufacturer and designation already exists.", "EngineAlreadyExists"));
        }

        existing.Manufacturer = request.Manufacturer.Trim();
        existing.Designation = request.Designation.Trim();
        existing.ImpulseClass = request.ImpulseClass.Trim().ToUpperInvariant();
        existing.TotalImpulseNs = request.TotalImpulseNs;
        existing.AverageThrustN = request.AverageThrustN;
        existing.DelayS = request.DelayS;
        existing.DiameterMm = request.DiameterMm;
        existing.LengthMm = request.LengthMm;
        existing.PropellantWeightG = request.PropellantWeightG;
        existing.TotalWeightG = request.TotalWeightG;
        existing.CaseType = request.CaseType.Trim();
        existing.PropellantType = request.PropellantType.Trim();
        existing.ThumbnailUrl = request.ThumbnailUrl.Trim();
        existing.ImageUrls = NormalizeStringList(request.ImageUrls);
        existing.CertificationBody = NormalizeNullable(request.CertificationBody);
        existing.IsActive = request.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _engines.ReplaceOneAsync(
            Builders<Engine>.Filter.Eq(item => item.Id, existing.Id),
            existing,
            cancellationToken: cancellationToken);

        return Ok(MapAdminEngine(existing));
    }

    [HttpDelete("engines/{id}")]
    public async Task<IActionResult> DeleteEngine(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var engineId))
        {
            return BadRequest(new ApiErrorResponse("Engine id is invalid.", "InvalidEngineId"));
        }

        var update = Builders<Engine>.Update
            .Set(item => item.IsActive, false)
            .Set(item => item.UpdatedAt, DateTime.UtcNow);

        var result = await _engines.UpdateOneAsync(
            Builders<Engine>.Filter.Eq(item => item.Id, engineId),
            update,
            cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            return NotFound(new ApiErrorResponse("Engine not found.", "EngineNotFound"));
        }

        return NoContent();
    }

    [HttpPost("images/upload")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadCatalogImage(
        [FromForm] IFormFile? file,
        [FromForm] string? scope,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new ApiErrorResponse("Image file is required.", "ImageFileRequired"));
        }

        var normalizedScope = string.IsNullOrWhiteSpace(scope)
            ? "catalog"
            : scope.Trim().ToLowerInvariant();

        if (!UploadScopeRegex.IsMatch(normalizedScope))
        {
            return BadRequest(new ApiErrorResponse("Upload scope is invalid.", "InvalidUploadScope"));
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".jpg";
        }

        var uploadsRoot = Path.Combine(AppContext.BaseDirectory, "Uploads", normalizedScope);
        Directory.CreateDirectory(uploadsRoot);

        var safeFileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsRoot, safeFileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return Ok(new UploadCatalogImageResponse(fullPath));
    }

    [HttpGet("seeds/export")]
    public async Task<IActionResult> ExportSeeds(CancellationToken cancellationToken = default)
    {
        var rockets = await _rockets
            .Find(FilterDefinition<Rocket>.Empty)
            .SortBy(item => item.Manufacturer)
            .ThenBy(item => item.Name)
            .ToListAsync(cancellationToken);

        var engines = await _engines
            .Find(FilterDefinition<Engine>.Empty)
            .SortBy(item => item.Manufacturer)
            .ThenBy(item => item.Designation)
            .ToListAsync(cancellationToken);

        var payload = new SeedExportResponse(
            rockets.Select(MapSeedRocket).ToArray(),
            engines.Select(MapSeedEngine).ToArray(),
            DateTime.UtcNow);

        return Ok(payload);
    }

    private static IActionResult? ValidateRocketRequest(AdminUpsertRocketRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Manufacturer))
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Manufacturer is required.", "ManufacturerRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.Sku))
        {
            return new BadRequestObjectResult(new ApiErrorResponse("SKU is required.", "SkuRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Name is required.", "NameRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Description is required.", "DescriptionRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.SkillLevel))
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Skill level is required.", "SkillLevelRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.FinMaterial))
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Fin material is required.", "FinMaterialRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.NoseCone))
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Nose cone is required.", "NoseConeRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.RecoverySystem))
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Recovery system is required.", "RecoverySystemRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.ThumbnailUrl))
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Thumbnail URL is required.", "ThumbnailUrlRequired"));
        }

        if (request.DiameterMm < 0 || request.LengthMm < 0 || request.WeightG < 0)
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Dimensions and weight must be non-negative.", "InvalidRocketMeasurements"));
        }

        return null;
    }

    private static IActionResult? ValidateEngineRequest(AdminUpsertEngineRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Manufacturer))
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Manufacturer is required.", "ManufacturerRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.Designation))
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Designation is required.", "DesignationRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.ImpulseClass))
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Impulse class is required.", "ImpulseClassRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.CaseType))
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Case type is required.", "CaseTypeRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.PropellantType))
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Propellant type is required.", "PropellantTypeRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.ThumbnailUrl))
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Thumbnail URL is required.", "ThumbnailUrlRequired"));
        }

        if (request.TotalImpulseNs < 0 ||
            request.AverageThrustN < 0 ||
            request.DelayS < 0 ||
            request.DiameterMm < 0 ||
            request.LengthMm < 0 ||
            request.PropellantWeightG < 0 ||
            request.TotalWeightG < 0)
        {
            return new BadRequestObjectResult(new ApiErrorResponse("Engine numeric values must be non-negative.", "InvalidEngineMeasurements"));
        }

        return null;
    }

    private static List<string> NormalizeStringList(IReadOnlyList<string>? values)
    {
        return (values ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static AdminRocketDto MapAdminRocket(Rocket rocket)
    {
        return new AdminRocketDto(
            rocket.Id.ToString(),
            rocket.Manufacturer,
            rocket.Sku,
            rocket.Name,
            rocket.Description,
            rocket.SkillLevel,
            rocket.RecommendedEngines,
            rocket.DiameterMm,
            rocket.LengthMm,
            rocket.WeightG,
            rocket.FinMaterial,
            rocket.NoseCone,
            rocket.RecoverySystem,
            rocket.ThumbnailUrl,
            rocket.ImageUrls,
            rocket.ProductUrl,
            rocket.IsActive,
            rocket.CreatedAt,
            rocket.UpdatedAt);
    }

    private static AdminEngineDto MapAdminEngine(Engine engine)
    {
        return new AdminEngineDto(
            engine.Id.ToString(),
            engine.Manufacturer,
            engine.Designation,
            engine.ImpulseClass,
            engine.TotalImpulseNs,
            engine.AverageThrustN,
            engine.DelayS,
            engine.DiameterMm,
            engine.LengthMm,
            engine.PropellantWeightG,
            engine.TotalWeightG,
            engine.CaseType,
            engine.PropellantType,
            engine.ThumbnailUrl,
            engine.ImageUrls,
            engine.CertificationBody,
            engine.IsActive,
            engine.CreatedAt,
            engine.UpdatedAt);
    }

    private static SeedRocketDto MapSeedRocket(Rocket rocket)
    {
        return new SeedRocketDto(
            rocket.Manufacturer,
            rocket.Sku,
            rocket.Name,
            rocket.Description,
            rocket.SkillLevel,
            rocket.RecommendedEngines,
            rocket.DiameterMm,
            rocket.LengthMm,
            rocket.WeightG,
            rocket.FinMaterial,
            rocket.NoseCone,
            rocket.RecoverySystem,
            rocket.ThumbnailUrl,
            rocket.ImageUrls,
            rocket.ProductUrl);
    }

    private static SeedEngineDto MapSeedEngine(Engine engine)
    {
        return new SeedEngineDto(
            engine.Manufacturer,
            engine.Designation,
            engine.ImpulseClass,
            engine.TotalImpulseNs,
            engine.AverageThrustN,
            engine.DelayS,
            engine.DiameterMm,
            engine.LengthMm,
            engine.PropellantWeightG,
            engine.TotalWeightG,
            engine.CaseType,
            engine.PropellantType,
            engine.ThumbnailUrl,
            engine.ImageUrls,
            engine.CertificationBody);
    }

    public sealed record AdminUpsertRocketRequest(
        string Manufacturer,
        string Sku,
        string Name,
        string Description,
        string SkillLevel,
        IReadOnlyList<string>? RecommendedEngines,
        double DiameterMm,
        double LengthMm,
        double WeightG,
        string FinMaterial,
        string NoseCone,
        string RecoverySystem,
        string ThumbnailUrl,
        IReadOnlyList<string>? ImageUrls,
        string? ProductUrl,
        bool IsActive = true);

    public sealed record AdminUpsertEngineRequest(
        string Manufacturer,
        string Designation,
        string ImpulseClass,
        double TotalImpulseNs,
        double AverageThrustN,
        double DelayS,
        double DiameterMm,
        double LengthMm,
        double PropellantWeightG,
        double TotalWeightG,
        string CaseType,
        string PropellantType,
        string ThumbnailUrl,
        IReadOnlyList<string>? ImageUrls,
        string? CertificationBody,
        bool IsActive = true);

    public sealed record AdminRocketDto(
        string Id,
        string Manufacturer,
        string Sku,
        string Name,
        string Description,
        string SkillLevel,
        IReadOnlyList<string> RecommendedEngines,
        double DiameterMm,
        double LengthMm,
        double WeightG,
        string FinMaterial,
        string NoseCone,
        string RecoverySystem,
        string ThumbnailUrl,
        IReadOnlyList<string> ImageUrls,
        string? ProductUrl,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public sealed record AdminEngineDto(
        string Id,
        string Manufacturer,
        string Designation,
        string ImpulseClass,
        double TotalImpulseNs,
        double AverageThrustN,
        double DelayS,
        double DiameterMm,
        double LengthMm,
        double PropellantWeightG,
        double TotalWeightG,
        string CaseType,
        string PropellantType,
        string ThumbnailUrl,
        IReadOnlyList<string> ImageUrls,
        string? CertificationBody,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public sealed record UploadCatalogImageResponse(string ImageUrl);

    public sealed record SeedExportResponse(
        IReadOnlyList<SeedRocketDto> Rockets,
        IReadOnlyList<SeedEngineDto> Engines,
        DateTime ExportedAtUtc);

    public sealed record SeedRocketDto(
        string Manufacturer,
        string Sku,
        string Name,
        string Description,
        string SkillLevel,
        IReadOnlyList<string> RecommendedEngines,
        double DiameterMm,
        double LengthMm,
        double WeightG,
        string FinMaterial,
        string NoseCone,
        string RecoverySystem,
        string ThumbnailUrl,
        IReadOnlyList<string> ImageUrls,
        string? ProductUrl);

    public sealed record SeedEngineDto(
        string Manufacturer,
        string Designation,
        string ImpulseClass,
        double TotalImpulseNs,
        double AverageThrustN,
        double DelayS,
        double DiameterMm,
        double LengthMm,
        double PropellantWeightG,
        double TotalWeightG,
        string CaseType,
        string PropellantType,
        string ThumbnailUrl,
        IReadOnlyList<string> ImageUrls,
        string? CertificationBody);
}
