using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RocketLog.Api.Models.Domain;

public abstract class BaseMongoEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
}