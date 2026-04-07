using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RocketLog.Api.Models.Common;

namespace RocketLog.Api.Models.Domain;

[BsonIgnoreExtraElements]
[BsonCollection("launches")]
public sealed class Launch : BaseMongoEntity
{
    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId UserId { get; set; }

    [BsonElement("userRocketId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId UserRocketId { get; set; }

    [BsonElement("engineId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId EngineId { get; set; }

    [BsonElement("launchDate")]
    public DateTime LaunchDate { get; set; } = DateTime.UtcNow;

    [BsonElement("location")]
    public LaunchLocation Location { get; set; } = new();

    [BsonElement("weather")]
    public LaunchWeather Weather { get; set; } = new();

    [BsonElement("outcome")]
    public string Outcome { get; set; } = "Success";

    [BsonElement("altitudeFt")]
    public double? AltitudeFt { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }

    [BsonElement("photoUrl")]
    public string? PhotoUrl { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class LaunchLocation
{
    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("lat")]
    public double Lat { get; set; }

    [BsonElement("lng")]
    public double Lng { get; set; }
}

public sealed class LaunchWeather
{
    [BsonElement("source")]
    public string Source { get; set; } = "api";

    [BsonElement("temperatureF")]
    public double TemperatureF { get; set; }

    [BsonElement("windSpeedMph")]
    public double WindSpeedMph { get; set; }

    [BsonElement("windDirection")]
    public string WindDirection { get; set; } = string.Empty;

    [BsonElement("humidity")]
    public double Humidity { get; set; }

    [BsonElement("conditions")]
    public string Conditions { get; set; } = string.Empty;

    [BsonElement("visibility_mi")]
    public double? VisibilityMi { get; set; }
}