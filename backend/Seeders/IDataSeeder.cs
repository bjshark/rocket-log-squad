namespace RocketLog.Api.Seeders;

public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}