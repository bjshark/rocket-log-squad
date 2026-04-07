using System.Text.Json;
using MongoDB.Driver;
using RocketLog.Api.Data;
using RocketLog.Api.Models.Domain;

namespace RocketLog.Api.Seeders;

public sealed class MongoDataSeeder : IDataSeeder
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly MongoDbContext _context;
    private readonly ILogger<MongoDataSeeder> _logger;

    public MongoDataSeeder(MongoDbContext context, ILogger<MongoDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.RunCommandAsync(
            (Command<MongoDB.Bson.BsonDocument>)"{ ping: 1 }",
            cancellationToken: cancellationToken);

        var users = _context.GetCollection<User>();
        var emailIndex = Builders<User>.IndexKeys.Ascending(user => user.Email);
        var emailIndexModel = new CreateIndexModel<User>(emailIndex, new CreateIndexOptions { Unique = true });

        await users.Indexes.CreateOneAsync(emailIndexModel, cancellationToken: cancellationToken);

        var rockets = _context.GetCollection<Rocket>();
        var engines = _context.GetCollection<Engine>();
        var userRockets = _context.GetCollection<UserRocket>();
        var userEngines = _context.GetCollection<UserEngine>();

        await EnsureCatalogIndexesAsync(rockets, engines, cancellationToken);
        await EnsureInventoryIndexesAsync(userRockets, userEngines, cancellationToken);

        var rocketSeeds = await ReadSeedFileAsync<List<Rocket>>("rockets.json", cancellationToken) ?? [];
        var engineSeeds = await ReadSeedFileAsync<List<Engine>>("engines.json", cancellationToken) ?? [];

        var seededRockets = await UpsertRocketsAsync(rockets, rocketSeeds, cancellationToken);
        var seededEngines = await UpsertEnginesAsync(engines, engineSeeds, cancellationToken);

        _logger.LogInformation(
            "Catalog seeding completed. Rockets upserted: {RocketCount}. Engines upserted: {EngineCount}.",
            seededRockets,
            seededEngines);
    }

    private static async Task EnsureCatalogIndexesAsync(
        IMongoCollection<Rocket> rockets,
        IMongoCollection<Engine> engines,
        CancellationToken cancellationToken)
    {
        var rocketUnique = new CreateIndexModel<Rocket>(
            Builders<Rocket>.IndexKeys
                .Ascending(rocket => rocket.Manufacturer)
                .Ascending(rocket => rocket.Sku),
            new CreateIndexOptions { Unique = true });

        var rocketSearch = new CreateIndexModel<Rocket>(
            Builders<Rocket>.IndexKeys
                .Ascending(rocket => rocket.Manufacturer)
                .Ascending(rocket => rocket.Name));

        await rockets.Indexes.CreateManyAsync([rocketUnique, rocketSearch], cancellationToken);

        var engineUnique = new CreateIndexModel<Engine>(
            Builders<Engine>.IndexKeys
                .Ascending(engine => engine.Manufacturer)
                .Ascending(engine => engine.Designation),
            new CreateIndexOptions { Unique = true });

        var engineSearch = new CreateIndexModel<Engine>(
            Builders<Engine>.IndexKeys
                .Ascending(engine => engine.ImpulseClass)
                .Ascending(engine => engine.Designation));

        await engines.Indexes.CreateManyAsync([engineUnique, engineSearch], cancellationToken);
    }

    private static async Task EnsureInventoryIndexesAsync(
        IMongoCollection<UserRocket> userRockets,
        IMongoCollection<UserEngine> userEngines,
        CancellationToken cancellationToken)
    {
        var userRocketUnique = new CreateIndexModel<UserRocket>(
            Builders<UserRocket>.IndexKeys
                .Ascending(rocket => rocket.UserId)
                .Ascending(rocket => rocket.RocketId),
            new CreateIndexOptions { Unique = true });

        await userRockets.Indexes.CreateOneAsync(userRocketUnique, cancellationToken: cancellationToken);

        var userEngineUnique = new CreateIndexModel<UserEngine>(
            Builders<UserEngine>.IndexKeys
                .Ascending(engine => engine.UserId)
                .Ascending(engine => engine.EngineId),
            new CreateIndexOptions { Unique = true });

        await userEngines.Indexes.CreateOneAsync(userEngineUnique, cancellationToken: cancellationToken);
    }

    private static async Task<int> UpsertRocketsAsync(
        IMongoCollection<Rocket> collection,
        IReadOnlyCollection<Rocket> seeds,
        CancellationToken cancellationToken)
    {
        if (seeds.Count == 0)
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        var writes = new List<WriteModel<Rocket>>(seeds.Count);

        foreach (var rocket in seeds)
        {
            rocket.UpdatedAt = now;

            var filter = Builders<Rocket>.Filter.And(
                Builders<Rocket>.Filter.Eq(existing => existing.Manufacturer, rocket.Manufacturer),
                Builders<Rocket>.Filter.Eq(existing => existing.Sku, rocket.Sku));

            var setOnInsert = Builders<Rocket>.Update.SetOnInsert(existing => existing.CreatedAt, now);

            var model = new UpdateOneModel<Rocket>(
                filter,
                Builders<Rocket>.Update.Combine(
                    setOnInsert,
                    Builders<Rocket>.Update.Set(existing => existing.Name, rocket.Name),
                    Builders<Rocket>.Update.Set(existing => existing.Description, rocket.Description),
                    Builders<Rocket>.Update.Set(existing => existing.SkillLevel, rocket.SkillLevel),
                    Builders<Rocket>.Update.Set(existing => existing.RecommendedEngines, rocket.RecommendedEngines),
                    Builders<Rocket>.Update.Set(existing => existing.DiameterMm, rocket.DiameterMm),
                    Builders<Rocket>.Update.Set(existing => existing.LengthMm, rocket.LengthMm),
                    Builders<Rocket>.Update.Set(existing => existing.WeightG, rocket.WeightG),
                    Builders<Rocket>.Update.Set(existing => existing.FinMaterial, rocket.FinMaterial),
                    Builders<Rocket>.Update.Set(existing => existing.NoseCone, rocket.NoseCone),
                    Builders<Rocket>.Update.Set(existing => existing.RecoverySystem, rocket.RecoverySystem),
                    Builders<Rocket>.Update.Set(existing => existing.ThumbnailUrl, rocket.ThumbnailUrl),
                    Builders<Rocket>.Update.Set(existing => existing.ImageUrls, rocket.ImageUrls),
                        Builders<Rocket>.Update.Set(existing => existing.ProductUrl, rocket.ProductUrl),
                        Builders<Rocket>.Update.Set(existing => existing.IsActive, rocket.IsActive),
                        Builders<Rocket>.Update.Set(existing => existing.UpdatedAt, now)));

                    model.IsUpsert = true;
                    writes.Add(model);
        }

        await collection.BulkWriteAsync(writes, new BulkWriteOptions { IsOrdered = false }, cancellationToken);
        return writes.Count;
    }

    private static async Task<int> UpsertEnginesAsync(
        IMongoCollection<Engine> collection,
        IReadOnlyCollection<Engine> seeds,
        CancellationToken cancellationToken)
    {
        if (seeds.Count == 0)
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        var writes = new List<WriteModel<Engine>>(seeds.Count);

        foreach (var engine in seeds)
        {
            engine.UpdatedAt = now;

            var filter = Builders<Engine>.Filter.And(
                Builders<Engine>.Filter.Eq(existing => existing.Manufacturer, engine.Manufacturer),
                Builders<Engine>.Filter.Eq(existing => existing.Designation, engine.Designation));

            var setOnInsert = Builders<Engine>.Update.SetOnInsert(existing => existing.CreatedAt, now);

            var model = new UpdateOneModel<Engine>(
                filter,
                Builders<Engine>.Update.Combine(
                    setOnInsert,
                    Builders<Engine>.Update.Set(existing => existing.ImpulseClass, engine.ImpulseClass),
                    Builders<Engine>.Update.Set(existing => existing.TotalImpulseNs, engine.TotalImpulseNs),
                    Builders<Engine>.Update.Set(existing => existing.AverageThrustN, engine.AverageThrustN),
                    Builders<Engine>.Update.Set(existing => existing.DelayS, engine.DelayS),
                    Builders<Engine>.Update.Set(existing => existing.DiameterMm, engine.DiameterMm),
                    Builders<Engine>.Update.Set(existing => existing.LengthMm, engine.LengthMm),
                    Builders<Engine>.Update.Set(existing => existing.PropellantWeightG, engine.PropellantWeightG),
                    Builders<Engine>.Update.Set(existing => existing.TotalWeightG, engine.TotalWeightG),
                    Builders<Engine>.Update.Set(existing => existing.CaseType, engine.CaseType),
                    Builders<Engine>.Update.Set(existing => existing.PropellantType, engine.PropellantType),
                    Builders<Engine>.Update.Set(existing => existing.ThumbnailUrl, engine.ThumbnailUrl),
                    Builders<Engine>.Update.Set(existing => existing.ImageUrls, engine.ImageUrls),
                        Builders<Engine>.Update.Set(existing => existing.CertificationBody, engine.CertificationBody),
                        Builders<Engine>.Update.Set(existing => existing.IsActive, engine.IsActive),
                        Builders<Engine>.Update.Set(existing => existing.UpdatedAt, now)));

                    model.IsUpsert = true;
                    writes.Add(model);
        }

        await collection.BulkWriteAsync(writes, new BulkWriteOptions { IsOrdered = false }, cancellationToken);
        return writes.Count;
    }

    private async Task<T?> ReadSeedFileAsync<T>(string fileName, CancellationToken cancellationToken)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "Seeds", fileName);

        if (!File.Exists(path))
        {
            _logger.LogWarning("Seed file not found: {Path}", path);
            return default;
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions, cancellationToken);
    }
}