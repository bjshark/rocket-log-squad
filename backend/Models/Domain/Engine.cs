using MongoDB.Bson.Serialization.Attributes;
using RocketLog.Api.Models.Common;

namespace RocketLog.Api.Models.Domain;

[BsonIgnoreExtraElements]
[BsonCollection("engines")]
public sealed class Engine : BaseMongoEntity
{
    [BsonElement("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    [BsonElement("designation")]
    public string Designation { get; set; } = string.Empty;

    [BsonElement("impulseClass")]
    public string ImpulseClass { get; set; } = string.Empty;

    [BsonElement("totalImpulse_Ns")]
    public double TotalImpulseNs { get; set; }

    [BsonElement("averageThrust_N")]
    public double AverageThrustN { get; set; }

    [BsonElement("delay_s")]
    public double DelayS { get; set; }

    [BsonElement("diameter_mm")]
    public double DiameterMm { get; set; }

    [BsonElement("length_mm")]
    public double LengthMm { get; set; }

    [BsonElement("propellantWeight_g")]
    public double PropellantWeightG { get; set; }

    [BsonElement("totalWeight_g")]
    public double TotalWeightG { get; set; }

    [BsonElement("caseType")]
    public string CaseType { get; set; } = string.Empty;

    [BsonElement("propellantType")]
    public string PropellantType { get; set; } = string.Empty;

    [BsonElement("thumbnailUrl")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [BsonElement("imageUrls")]
    public List<string> ImageUrls { get; set; } = [];

    [BsonElement("certificationBody")]
    public string? CertificationBody { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}