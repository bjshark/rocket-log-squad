using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RocketLog.Api.Models.Common;

namespace RocketLog.Api.Models.Domain;

[BsonIgnoreExtraElements]
[BsonCollection("user_rockets")]
public sealed class UserRocket : BaseMongoEntity
{
    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId UserId { get; set; }

    [BsonElement("rocketId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId RocketId { get; set; }

    [BsonElement("nickname")]
    public string? Nickname { get; set; }

    [BsonElement("buildDate")]
    public DateTime? BuildDate { get; set; }

    [BsonElement("condition")]
    public string Condition { get; set; } = "New";

    [BsonElement("buildNotes")]
    public string? BuildNotes { get; set; }

    [BsonElement("photoUrl")]
    public string? PhotoUrl { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}