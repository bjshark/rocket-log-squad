using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RocketLog.Api.Models.Configuration;
using RocketLog.Api.Models.Domain;

namespace RocketLog.Api.Data;

public sealed class MongoDbContext
{
    public MongoDbContext(IMongoClient client, IOptions<MongoDbOptions> options)
    {
        Client = client;
        Options = options.Value;
        Database = client.GetDatabase(Options.DatabaseName);
    }

    public IMongoClient Client { get; }

    public MongoDbOptions Options { get; }

    public IMongoDatabase Database { get; }

    public IMongoCollection<TDocument> GetCollection<TDocument>() where TDocument : BaseMongoEntity
    {
        var collectionName = MongoCollectionResolver.GetCollectionName<TDocument>();
        return Database.GetCollection<TDocument>(collectionName);
    }
}