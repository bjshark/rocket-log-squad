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
[Route("api/v1/engines")]
public sealed class EnginesController : ControllerBase
{
    private readonly IMongoCollection<Engine> _engines;

    public EnginesController(MongoDbContext context)
    {
        _engines = context.GetCollection<Engine>();
    }

    [HttpGet("filters")]
    public async Task<IActionResult> GetEngineFilters(CancellationToken cancellationToken = default)
    {
        var activeFilter = Builders<Engine>.Filter.Eq(engine => engine.IsActive, true);

        var manufacturers = await _engines
            .Distinct<string>("manufacturer", activeFilter)
            .ToListAsync(cancellationToken);

        var impulseClasses = await _engines
            .Distinct<string>("impulseClass", activeFilter)
            .ToListAsync(cancellationToken);

        return Ok(new EngineFilterOptionsDto(
            manufacturers
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            impulseClasses
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim().ToUpperInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray()));
    }

    [HttpGet]
    public async Task<IActionResult> GetEngines(
        [FromQuery] string? query,
        [FromQuery] string? manufacturer,
        [FromQuery] string? impulseClass,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var validatedPage = page < 1 ? 1 : page;
        var validatedPageSize = Math.Clamp(pageSize, 1, 100);

        var filters = new List<FilterDefinition<Engine>>
        {
            Builders<Engine>.Filter.Eq(engine => engine.IsActive, true)
        };

        if (!string.IsNullOrWhiteSpace(manufacturer))
        {
            filters.Add(Builders<Engine>.Filter.Eq(engine => engine.Manufacturer, manufacturer.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(impulseClass))
        {
            filters.Add(Builders<Engine>.Filter.Eq(engine => engine.ImpulseClass, impulseClass.Trim().ToUpperInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = query.Trim();
            var regex = new BsonRegularExpression(pattern, "i");

            filters.Add(
                Builders<Engine>.Filter.Or(
                    Builders<Engine>.Filter.Regex(engine => engine.Designation, regex),
                    Builders<Engine>.Filter.Regex(engine => engine.Manufacturer, regex),
                    Builders<Engine>.Filter.Regex(engine => engine.PropellantType, regex)));
        }

        var filter = Builders<Engine>.Filter.And(filters);
        var total = await _engines.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await _engines
            .Find(filter)
            .SortBy(engine => engine.ImpulseClass)
            .ThenBy(engine => engine.Designation)
            .Skip((validatedPage - 1) * validatedPageSize)
            .Limit(validatedPageSize)
            .ToListAsync(cancellationToken);

        return Ok(new PagedResponse<EngineCatalogItemDto>(
            items.Select(MapEngine).ToArray(),
            total,
            validatedPage,
            validatedPageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEngineById(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return BadRequest(new ApiErrorResponse("Engine id is invalid.", "InvalidEngineId"));
        }

        var filter = Builders<Engine>.Filter.And(
            Builders<Engine>.Filter.Eq(engine => engine.Id, objectId),
            Builders<Engine>.Filter.Eq(engine => engine.IsActive, true));

        var engine = await _engines.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (engine is null)
        {
            return NotFound(new ApiErrorResponse("Engine not found.", "EngineNotFound"));
        }

        return Ok(MapEngine(engine));
    }

    private static EngineCatalogItemDto MapEngine(Engine engine)
    {
        return new EngineCatalogItemDto(
            engine.Id.ToString(),
            engine.Manufacturer,
            engine.Designation,
            engine.ImpulseClass,
            engine.TotalImpulseNs,
            engine.AverageThrustN,
            engine.DelayS,
            engine.CaseType,
            engine.PropellantType,
            engine.ThumbnailUrl,
            engine.CertificationBody);
    }

    public sealed record EngineCatalogItemDto(
        string Id,
        string Manufacturer,
        string Designation,
        string ImpulseClass,
        double TotalImpulseNs,
        double AverageThrustN,
        double DelayS,
        string CaseType,
        string PropellantType,
        string ThumbnailUrl,
        string? CertificationBody);

    public sealed record EngineFilterOptionsDto(
        IReadOnlyList<string> Manufacturers,
        IReadOnlyList<string> ImpulseClasses);
}
