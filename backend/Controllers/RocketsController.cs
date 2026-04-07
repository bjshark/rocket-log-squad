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
[Route("api/v1/rockets")]
public sealed class RocketsController : ControllerBase
{
    private readonly IMongoCollection<Rocket> _rockets;

    public RocketsController(MongoDbContext context)
    {
        _rockets = context.GetCollection<Rocket>();
    }

    [HttpGet("filters")]
    public async Task<IActionResult> GetRocketFilters(CancellationToken cancellationToken = default)
    {
        var manufacturerFilter = Builders<Rocket>.Filter.Eq(rocket => rocket.IsActive, true);

        var manufacturers = await _rockets
            .Distinct<string>("manufacturer", manufacturerFilter)
            .ToListAsync(cancellationToken);

        return Ok(new RocketFilterOptionsDto(
            manufacturers
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray()));
    }

    [HttpGet]
    public async Task<IActionResult> GetRockets(
        [FromQuery] string? query,
        [FromQuery] string? manufacturer,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var validatedPage = page < 1 ? 1 : page;
        var validatedPageSize = Math.Clamp(pageSize, 1, 100);

        var filters = new List<FilterDefinition<Rocket>>
        {
            Builders<Rocket>.Filter.Eq(rocket => rocket.IsActive, true)
        };

        if (!string.IsNullOrWhiteSpace(manufacturer))
        {
            filters.Add(Builders<Rocket>.Filter.Eq(rocket => rocket.Manufacturer, manufacturer.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = query.Trim();
            var regex = new BsonRegularExpression(pattern, "i");

            filters.Add(
                Builders<Rocket>.Filter.Or(
                    Builders<Rocket>.Filter.Regex(rocket => rocket.Name, regex),
                    Builders<Rocket>.Filter.Regex(rocket => rocket.Description, regex),
                    Builders<Rocket>.Filter.Regex(rocket => rocket.Sku, regex)));
        }

        var filter = Builders<Rocket>.Filter.And(filters);
        var total = await _rockets.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await _rockets
            .Find(filter)
            .SortBy(rocket => rocket.Manufacturer)
            .ThenBy(rocket => rocket.Name)
            .Skip((validatedPage - 1) * validatedPageSize)
            .Limit(validatedPageSize)
            .ToListAsync(cancellationToken);

        return Ok(new PagedResponse<RocketCatalogItemDto>(
            items.Select(MapRocket).ToArray(),
            total,
            validatedPage,
            validatedPageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRocketById(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return BadRequest(new ApiErrorResponse("Rocket id is invalid.", "InvalidRocketId"));
        }

        var filter = Builders<Rocket>.Filter.And(
            Builders<Rocket>.Filter.Eq(rocket => rocket.Id, objectId),
            Builders<Rocket>.Filter.Eq(rocket => rocket.IsActive, true));

        var rocket = await _rockets.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (rocket is null)
        {
            return NotFound(new ApiErrorResponse("Rocket not found.", "RocketNotFound"));
        }

        return Ok(MapRocket(rocket));
    }

    private static RocketCatalogItemDto MapRocket(Rocket rocket)
    {
        return new RocketCatalogItemDto(
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
            rocket.ThumbnailUrl,
            rocket.ProductUrl);
    }

    public sealed record RocketCatalogItemDto(
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
        string ThumbnailUrl,
        string? ProductUrl);

    public sealed record RocketFilterOptionsDto(IReadOnlyList<string> Manufacturers);
}
