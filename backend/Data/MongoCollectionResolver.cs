using RocketLog.Api.Models.Common;
using RocketLog.Api.Models.Domain;

namespace RocketLog.Api.Data;

internal static class MongoCollectionResolver
{
    public static string GetCollectionName<TDocument>() where TDocument : BaseMongoEntity
    {
        var attribute = typeof(TDocument)
            .GetCustomAttributes(typeof(BsonCollectionAttribute), inherit: false)
            .Cast<BsonCollectionAttribute>()
            .SingleOrDefault();

        return attribute?.CollectionName
            ?? throw new InvalidOperationException(
                $"Mongo collection name is not configured for {typeof(TDocument).Name}.");
    }
}