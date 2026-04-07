using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace RocketLog.Api.IntegrationTests;

public sealed class InventoryEndpointsTests : IClassFixture<CatalogApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;

    public InventoryEndpointsTests(CatalogApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MyRockets_SupportsAddUpdateUploadAndDelete()
    {
        var rocketCatalog = await _client.GetFromJsonAsync<PagedResponse<RocketCatalogItem>>(
            "/api/v1/rockets?page=1&pageSize=1",
            JsonOptions);

        Assert.NotNull(rocketCatalog);
        var rocket = Assert.Single(rocketCatalog.Items);

        var addResponse = await _client.PostAsJsonAsync("/api/v1/my/rockets", new { rocketId = rocket.Id });
        addResponse.EnsureSuccessStatusCode();

        var addPayload = await addResponse.Content.ReadFromJsonAsync<AddRocketPayload>(JsonOptions);
        Assert.NotNull(addPayload);

        var listResponse = await _client.GetAsync("/api/v1/my/rockets");
        listResponse.EnsureSuccessStatusCode();

        var listPayload = await listResponse.Content.ReadFromJsonAsync<List<UserRocketItem>>(JsonOptions);
        Assert.NotNull(listPayload);
        var created = Assert.Single(listPayload);
        Assert.Equal(addPayload.UserRocketId, created.Id);

        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/my/rockets/{created.Id}", new
        {
            nickname = "Weekend Flyer",
            buildDate = "2026-04-01",
            condition = "Good",
            buildNotes = "Refinished fins",
            photoUrl = "https://example.test/photo.jpg"
        });

        updateResponse.EnsureSuccessStatusCode();
        var updatedPayload = await updateResponse.Content.ReadFromJsonAsync<UserRocketItem>(JsonOptions);

        Assert.NotNull(updatedPayload);
        Assert.Equal("Weekend Flyer", updatedPayload.Nickname);
        Assert.Equal("Good", updatedPayload.Condition);

        using var content = new MultipartFormDataContent();
        var fileBytes = Encoding.UTF8.GetBytes("fake-image");
        content.Add(new ByteArrayContent(fileBytes), "file", "build-photo.jpg");

        var uploadResponse = await _client.PostAsync($"/api/v1/my/rockets/{created.Id}/photo", content);
        uploadResponse.EnsureSuccessStatusCode();

        var uploadPayload = await uploadResponse.Content.ReadFromJsonAsync<UploadPhotoPayload>(JsonOptions);
        Assert.NotNull(uploadPayload);
        Assert.Equal(created.Id, uploadPayload.UserRocketId);
        Assert.False(string.IsNullOrWhiteSpace(uploadPayload.PhotoUrl));

        var deleteResponse = await _client.DeleteAsync($"/api/v1/my/rockets/{created.Id}");
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var finalList = await _client.GetFromJsonAsync<List<UserRocketItem>>("/api/v1/my/rockets", JsonOptions);
        Assert.NotNull(finalList);
        Assert.Empty(finalList);
    }

    [Fact]
    public async Task MyEngines_SupportsAddUpdateAndDelete()
    {
        var engineCatalog = await _client.GetFromJsonAsync<PagedResponse<EngineCatalogItem>>(
            "/api/v1/engines?page=1&pageSize=1",
            JsonOptions);

        Assert.NotNull(engineCatalog);
        var engine = Assert.Single(engineCatalog.Items);

        var addResponse = await _client.PostAsJsonAsync("/api/v1/my/engines", new { engineId = engine.Id });
        addResponse.EnsureSuccessStatusCode();

        var addPayload = await addResponse.Content.ReadFromJsonAsync<AddEnginePayload>(JsonOptions);
        Assert.NotNull(addPayload);

        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/my/engines/{addPayload.UserEngineId}", new
        {
            quantityOnHand = 6,
            purchaseDate = "2026-03-28",
            notes = "Club purchase"
        });

        updateResponse.EnsureSuccessStatusCode();
        var updatePayload = await updateResponse.Content.ReadFromJsonAsync<UserEngineItem>(JsonOptions);

        Assert.NotNull(updatePayload);
        Assert.Equal(6, updatePayload.QuantityOnHand);
        Assert.Equal("Club purchase", updatePayload.Notes);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/my/engines/{addPayload.UserEngineId}");
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var finalList = await _client.GetFromJsonAsync<List<UserEngineItem>>("/api/v1/my/engines", JsonOptions);
        Assert.NotNull(finalList);
        Assert.Empty(finalList);
    }

    [Fact]
    public async Task Accessories_SupportsCreateUpdateAndDelete()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/my/accessories", new
        {
            name = "Launch Controller",
            category = "Electronics",
            brand = "Estes",
            notes = "Spare battery pack",
            photoUrl = "https://example.test/accessory.jpg"
        });

        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<AccessoryItem>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal("Launch Controller", created.Name);

        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/my/accessories/{created.Id}", new
        {
            name = "Launch Controller Pro",
            category = "Electronics",
            brand = "Estes",
            notes = "Updated notes",
            photoUrl = "https://example.test/accessory-updated.jpg"
        });

        updateResponse.EnsureSuccessStatusCode();

        var updated = await updateResponse.Content.ReadFromJsonAsync<AccessoryItem>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal("Launch Controller Pro", updated.Name);

        var list = await _client.GetFromJsonAsync<List<AccessoryItem>>("/api/v1/my/accessories", JsonOptions);
        Assert.NotNull(list);
        Assert.Single(list);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/my/accessories/{created.Id}");
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var finalList = await _client.GetFromJsonAsync<List<AccessoryItem>>("/api/v1/my/accessories", JsonOptions);
        Assert.NotNull(finalList);
        Assert.Empty(finalList);
    }

    public sealed record PagedResponse<T>(IReadOnlyList<T> Items, long Total, int Page, int PageSize);

    public sealed record RocketCatalogItem(string Id);

    public sealed record EngineCatalogItem(string Id);

    public sealed record AddRocketPayload(string UserRocketId, string RocketId, bool Added);

    public sealed record AddEnginePayload(string UserEngineId, string EngineId, int QuantityOnHand, bool AddedNewEntry);

    public sealed record UploadPhotoPayload(string UserRocketId, string? PhotoUrl);

    public sealed record UserRocketItem(string Id, string? Nickname, string Condition);

    public sealed record UserEngineItem(string Id, int QuantityOnHand, string? Notes);

    public sealed record AccessoryItem(string Id, string Name);
}
