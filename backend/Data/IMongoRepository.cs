using MongoDB.Bson;
using RocketLog.Api.Models.Domain;

namespace RocketLog.Api.Data;

public interface IMongoRepository<TDocument> where TDocument : BaseMongoEntity
{
    Task<IReadOnlyList<TDocument>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TDocument?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken = default);

    Task InsertAsync(TDocument document, CancellationToken cancellationToken = default);
}