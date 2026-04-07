using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Mongo2Go;
using Xunit;

namespace RocketLog.Api.IntegrationTests;

public sealed class CatalogEndpointsTests : IClassFixture<CatalogApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;

    public CatalogEndpointsTests(CatalogApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRockets_FiltersByManufacturer_AndPaginates()
    {
        var response = await _client.GetAsync("/api/v1/rockets?manufacturer=Estes&page=1&pageSize=3");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PagedResponse<RocketItem>>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Equal(1, payload.Page);
        Assert.Equal(3, payload.PageSize);
        Assert.True(payload.Total >= payload.Items.Count);
        Assert.True(payload.Items.Count <= 3);
        Assert.All(payload.Items, item => Assert.Equal("Estes", item.Manufacturer));
    }

    [Fact]
    public async Task GetRockets_NormalizesInvalidPageNumberToOne()
    {
        var response = await _client.GetAsync("/api/v1/rockets?page=0&pageSize=2");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PagedResponse<RocketItem>>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Equal(1, payload.Page);
        Assert.True(payload.Items.Count <= 2);
    }

    [Fact]
    public async Task GetEngines_FiltersByImpulseClass_CaseInsensitive()
    {
        var response = await _client.GetAsync("/api/v1/engines?impulseClass=c&page=1&pageSize=4");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PagedResponse<EngineItem>>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Equal(1, payload.Page);
        Assert.Equal(4, payload.PageSize);
        Assert.True(payload.Items.Count > 0);
        Assert.All(payload.Items, item => Assert.Equal("C", item.ImpulseClass));
    }

    [Fact]
    public async Task GetEngines_ClampsPageSizeToMaximum()
    {
        var response = await _client.GetAsync("/api/v1/engines?page=1&pageSize=500");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PagedResponse<EngineItem>>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Equal(100, payload.PageSize);
        Assert.True(payload.Items.Count <= 100);
    }

    public sealed record PagedResponse<T>(IReadOnlyList<T> Items, long Total, int Page, int PageSize);

    public sealed record RocketItem(string Manufacturer);

    public sealed record EngineItem(string ImpulseClass);
}

public sealed class CatalogApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private MongoDbRunner? _mongoRunner;
    private string _databaseName = string.Empty;

    public Task InitializeAsync()
    {
        _mongoRunner = MongoDbRunner.Start(singleNodeReplSet: true);
        _databaseName = $"rocket_log_it_{Guid.NewGuid():N}";
        return Task.CompletedTask;
    }

    public new Task DisposeAsync()
    {
        _mongoRunner?.Dispose();
        return Task.CompletedTask;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            if (_mongoRunner is null)
            {
                throw new InvalidOperationException("Mongo2Go runner was not initialized.");
            }

            var inMemorySettings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:MongoDb"] = _mongoRunner.ConnectionString,
                ["MongoDb:ConnectionString"] = _mongoRunner.ConnectionString,
                ["MongoDb:DatabaseName"] = _databaseName,
                ["Auth:Enabled"] = "false"
            };

            configurationBuilder.AddInMemoryCollection(inMemorySettings);
        });
    }
}
