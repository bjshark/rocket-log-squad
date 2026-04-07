using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RocketLog.Api.Models.Common;

namespace RocketLog.Api.Models.Domain;

[BsonIgnoreExtraElements]
[BsonCollection("user_engines")]
public sealed class UserEngine : BaseMongoEntity
{
    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId UserId { get; set; }

    [BsonElement("engineId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId EngineId { get; set; }

    [BsonElement("quantityOnHand")]
    public int QuantityOnHand { get; set; }

    [BsonElement("purchaseDate")]
    public DateTime? PurchaseDate { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}