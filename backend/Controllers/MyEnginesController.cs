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
[Route("api/v1/my/engines")]
public sealed class MyEnginesController : ControllerBase
{
    private readonly IMongoCollection<Engine> _engines;
    private readonly IMongoCollection<UserEngine> _userEngines;

    public MyEnginesController(MongoDbContext context)
    {
        _engines = context.GetCollection<Engine>();
        _userEngines = context.GetCollection<UserEngine>();
    }

    [HttpPost]
    public async Task<IActionResult> AddEngineFromCatalog(
        [FromBody] AddEngineFromCatalogRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.EngineId))
        {
            return BadRequest(new ApiErrorResponse("Engine id is required.", "EngineIdRequired"));
        }

        if (!ObjectId.TryParse(request.EngineId, out var engineId))
        {
            return BadRequest(new ApiErrorResponse("Engine id is invalid.", "InvalidEngineId"));
        }

        var engineExists = await _engines
            .Find(Builders<Engine>.Filter.And(
                Builders<Engine>.Filter.Eq(engine => engine.Id, engineId),
                Builders<Engine>.Filter.Eq(engine => engine.IsActive, true)))
            .AnyAsync(cancellationToken);

        if (!engineExists)
        {
            return NotFound(new ApiErrorResponse("Engine not found.", "EngineNotFound"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var existing = await _userEngines
            .Find(Builders<UserEngine>.Filter.And(
                Builders<UserEngine>.Filter.Eq(engine => engine.UserId, userId),
                Builders<UserEngine>.Filter.Eq(engine => engine.EngineId, engineId)))
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            existing.QuantityOnHand += 1;
            existing.UpdatedAt = DateTime.UtcNow;

            await _userEngines.ReplaceOneAsync(
                Builders<UserEngine>.Filter.Eq(engine => engine.Id, existing.Id),
                existing,
                cancellationToken: cancellationToken);

            return Ok(new AddEngineFromCatalogResponse(
                existing.Id.ToString(),
                existing.EngineId.ToString(),
                existing.QuantityOnHand,
                false));
        }

        var userEngine = new UserEngine
        {
            UserId = userId,
            EngineId = engineId,
            QuantityOnHand = 1,
            UpdatedAt = DateTime.UtcNow
        };

        await _userEngines.InsertOneAsync(userEngine, cancellationToken: cancellationToken);

        return Ok(new AddEngineFromCatalogResponse(
            userEngine.Id.ToString(),
            userEngine.EngineId.ToString(),
            userEngine.QuantityOnHand,
            true));
    }

    [HttpGet]
    public async Task<IActionResult> GetMyEngines(CancellationToken cancellationToken = default)
    {
        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var filter = Builders<UserEngine>.Filter.Eq(engine => engine.UserId, userId);

        var userEngines = await _userEngines
            .Find(filter)
            .SortByDescending(engine => engine.UpdatedAt)
            .ToListAsync(cancellationToken);

        var engineIds = userEngines
            .Select(engine => engine.EngineId)
            .Distinct()
            .ToList();

        var engines = engineIds.Count == 0
            ? []
            : await _engines
                .Find(Builders<Engine>.Filter.In(engine => engine.Id, engineIds))
                .ToListAsync(cancellationToken);

        var engineById = engines.ToDictionary(engine => engine.Id, engine => engine);

        var items = userEngines
            .Select(userEngine =>
            {
                engineById.TryGetValue(userEngine.EngineId, out var engine);
                return MapUserEngine(userEngine, engine);
            })
            .ToArray();

        return Ok(items);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMyEngine(
        string id,
        [FromBody] UpdateUserEngineRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest(new ApiErrorResponse("Request body is required.", "RequestRequired"));
        }

        if (!ObjectId.TryParse(id, out var userEngineId))
        {
            return BadRequest(new ApiErrorResponse("User engine id is invalid.", "InvalidUserEngineId"));
        }

        if (request.QuantityOnHand < 0)
        {
            return BadRequest(new ApiErrorResponse("Quantity cannot be negative.", "InvalidQuantityOnHand"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var userEngine = await _userEngines
            .Find(Builders<UserEngine>.Filter.And(
                Builders<UserEngine>.Filter.Eq(engine => engine.Id, userEngineId),
                Builders<UserEngine>.Filter.Eq(engine => engine.UserId, userId)))
            .FirstOrDefaultAsync(cancellationToken);

        if (userEngine is null)
        {
            return NotFound(new ApiErrorResponse("User engine not found.", "UserEngineNotFound"));
        }

        userEngine.QuantityOnHand = request.QuantityOnHand;
        userEngine.PurchaseDate = request.PurchaseDate;
        userEngine.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        userEngine.UpdatedAt = DateTime.UtcNow;

        await _userEngines.ReplaceOneAsync(
            Builders<UserEngine>.Filter.Eq(engine => engine.Id, userEngine.Id),
            userEngine,
            cancellationToken: cancellationToken);

        var engine = await _engines
            .Find(Builders<Engine>.Filter.Eq(item => item.Id, userEngine.EngineId))
            .FirstOrDefaultAsync(cancellationToken);

        return Ok(MapUserEngine(userEngine, engine));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMyEngine(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var userEngineId))
        {
            return BadRequest(new ApiErrorResponse("User engine id is invalid.", "InvalidUserEngineId"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var deleted = await _userEngines.DeleteOneAsync(
            Builders<UserEngine>.Filter.And(
                Builders<UserEngine>.Filter.Eq(engine => engine.Id, userEngineId),
                Builders<UserEngine>.Filter.Eq(engine => engine.UserId, userId)),
            cancellationToken);

        if (deleted.DeletedCount == 0)
        {
            return NotFound(new ApiErrorResponse("User engine not found.", "UserEngineNotFound"));
        }

        return NoContent();
    }

    private static UserEngineItemDto MapUserEngine(UserEngine userEngine, Engine? engine)
    {
        return new UserEngineItemDto(
            userEngine.Id.ToString(),
            userEngine.EngineId.ToString(),
            engine?.Manufacturer,
            engine?.Designation,
            engine?.ImpulseClass,
            engine?.ThumbnailUrl,
            userEngine.QuantityOnHand,
            userEngine.PurchaseDate,
            userEngine.Notes,
            userEngine.UpdatedAt);
    }

    public sealed record AddEngineFromCatalogRequest(string EngineId);

    public sealed record AddEngineFromCatalogResponse(
        string UserEngineId,
        string EngineId,
        int QuantityOnHand,
        bool AddedNewEntry);

    public sealed record UpdateUserEngineRequest(
        int QuantityOnHand,
        DateTime? PurchaseDate,
        string? Notes);

    public sealed record UserEngineItemDto(
        string Id,
        string EngineId,
        string? Manufacturer,
        string? Designation,
        string? ImpulseClass,
        string? ThumbnailUrl,
        int QuantityOnHand,
        DateTime? PurchaseDate,
        string? Notes,
        DateTime UpdatedAt);
}