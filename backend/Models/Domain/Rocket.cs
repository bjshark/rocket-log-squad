using MongoDB.Bson.Serialization.Attributes;
using RocketLog.Api.Models.Common;

namespace RocketLog.Api.Models.Domain;

[BsonIgnoreExtraElements]
[BsonCollection("rockets")]
public sealed class Rocket : BaseMongoEntity
{
    [BsonElement("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    [BsonElement("sku")]
    public string Sku { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("skillLevel")]
    public string SkillLevel { get; set; } = string.Empty;

    [BsonElement("recommendedEngines")]
    public List<string> RecommendedEngines { get; set; } = [];

    [BsonElement("diameter_mm")]
    public double DiameterMm { get; set; }

    [BsonElement("length_mm")]
    public double LengthMm { get; set; }

    [BsonElement("weight_g")]
    public double WeightG { get; set; }

    [BsonElement("finMaterial")]
    public string FinMaterial { get; set; } = string.Empty;

    [BsonElement("noseCone")]
    public string NoseCone { get; set; } = string.Empty;

    [BsonElement("recoverySystem")]
    public string RecoverySystem { get; set; } = string.Empty;

    [BsonElement("thumbnailUrl")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [BsonElement("imageUrls")]
    public List<string> ImageUrls { get; set; } = [];

    [BsonElement("productUrl")]
    public string? ProductUrl { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}