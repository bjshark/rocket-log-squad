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
[Route("api/v1/my/accessories")]
public sealed class MyAccessoriesController : ControllerBase
{
    private readonly IMongoCollection<Accessory> _accessories;

    public MyAccessoriesController(MongoDbContext context)
    {
        _accessories = context.GetCollection<Accessory>();
    }

    [HttpGet]
    public async Task<IActionResult> GetMyAccessories(CancellationToken cancellationToken = default)
    {
        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var items = await _accessories
            .Find(Builders<Accessory>.Filter.Eq(item => item.UserId, userId))
            .SortByDescending(item => item.UpdatedAt)
            .ToListAsync(cancellationToken);

        return Ok(items.Select(MapAccessory).ToArray());
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccessory(
        [FromBody] CreateAccessoryRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest(new ApiErrorResponse("Request body is required.", "RequestRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ApiErrorResponse("Accessory name is required.", "AccessoryNameRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return BadRequest(new ApiErrorResponse("Accessory category is required.", "AccessoryCategoryRequired"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var now = DateTime.UtcNow;
        var accessory = new Accessory
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl) ? null : request.PhotoUrl.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _accessories.InsertOneAsync(accessory, cancellationToken: cancellationToken);

        return Ok(MapAccessory(accessory));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccessory(
        string id,
        [FromBody] UpdateAccessoryRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest(new ApiErrorResponse("Request body is required.", "RequestRequired"));
        }

        if (!ObjectId.TryParse(id, out var accessoryId))
        {
            return BadRequest(new ApiErrorResponse("Accessory id is invalid.", "InvalidAccessoryId"));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ApiErrorResponse("Accessory name is required.", "AccessoryNameRequired"));
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return BadRequest(new ApiErrorResponse("Accessory category is required.", "AccessoryCategoryRequired"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var accessory = await _accessories
            .Find(Builders<Accessory>.Filter.And(
                Builders<Accessory>.Filter.Eq(item => item.Id, accessoryId),
                Builders<Accessory>.Filter.Eq(item => item.UserId, userId)))
            .FirstOrDefaultAsync(cancellationToken);

        if (accessory is null)
        {
            return NotFound(new ApiErrorResponse("Accessory not found.", "AccessoryNotFound"));
        }

        accessory.Name = request.Name.Trim();
        accessory.Category = request.Category.Trim();
        accessory.Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim();
        accessory.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        accessory.PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl) ? null : request.PhotoUrl.Trim();
        accessory.UpdatedAt = DateTime.UtcNow;

        await _accessories.ReplaceOneAsync(
            Builders<Accessory>.Filter.Eq(item => item.Id, accessory.Id),
            accessory,
            cancellationToken: cancellationToken);

        return Ok(MapAccessory(accessory));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccessory(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var accessoryId))
        {
            return BadRequest(new ApiErrorResponse("Accessory id is invalid.", "InvalidAccessoryId"));
        }

        if (!InventoryUserResolver.TryResolveUserId(User, out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is invalid.", "InvalidUserIdentity"));
        }

        var deleted = await _accessories.DeleteOneAsync(
            Builders<Accessory>.Filter.And(
                Builders<Accessory>.Filter.Eq(item => item.Id, accessoryId),
                Builders<Accessory>.Filter.Eq(item => item.UserId, userId)),
            cancellationToken);

        if (deleted.DeletedCount == 0)
        {
            return NotFound(new ApiErrorResponse("Accessory not found.", "AccessoryNotFound"));
        }

        return NoContent();
    }

    private static AccessoryItemDto MapAccessory(Accessory accessory)
    {
        return new AccessoryItemDto(
            accessory.Id.ToString(),
            accessory.Name,
            accessory.Category,
            accessory.Brand,
            accessory.Notes,
            accessory.PhotoUrl,
            accessory.CreatedAt,
            accessory.UpdatedAt);
    }

    public sealed record CreateAccessoryRequest(
        string Name,
        string Category,
        string? Brand,
        string? Notes,
        string? PhotoUrl);

    public sealed record UpdateAccessoryRequest(
        string Name,
        string Category,
        string? Brand,
        string? Notes,
        string? PhotoUrl);

    public sealed record AccessoryItemDto(
        string Id,
        string Name,
        string Category,
        string? Brand,
        string? Notes,
        string? PhotoUrl,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}