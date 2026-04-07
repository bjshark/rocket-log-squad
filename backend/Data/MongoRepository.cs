using MongoDB.Bson;
using MongoDB.Driver;
using RocketLog.Api.Models.Domain;

namespace RocketLog.Api.Data;

public sealed class MongoRepository<TDocument> : IMongoRepository<TDocument>
    where TDocument : BaseMongoEntity
{
    private readonly IMongoCollection<TDocument> _collection;

    public MongoRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<TDocument>();
    }

    public async Task<IReadOnlyList<TDocument>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(FilterDefinition<TDocument>.Empty)
            .ToListAsync(cancellationToken);
    }

    public async Task<TDocument?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(document => document.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task InsertAsync(TDocument document, CancellationToken cancellationToken = default)
    {
        return _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }
}