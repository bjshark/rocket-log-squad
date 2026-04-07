using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace RocketLog.Api.IntegrationTests;

public sealed class AdminEndpointsTests : IClassFixture<CatalogApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;

    public AdminEndpointsTests(CatalogApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AdminRockets_SupportsCreateUpdateDeleteAndExport()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/admin/rockets", new
        {
            manufacturer = "TestCo",
            sku = "TL-100",
            name = "Trailblazer",
            description = "Phase 6 admin CRUD test rocket",
            skillLevel = "Beginner",
            recommendedEngines = new[] { "A8-3", "B6-4" },
            diameterMm = 24,
            lengthMm = 400,
            weightG = 68,
            finMaterial = "Balsa",
            noseCone = "Plastic",
            recoverySystem = "Parachute",
            thumbnailUrl = "https://example.test/rocket-thumb.jpg",
            imageUrls = new[] { "https://example.test/rocket-1.jpg" },
            productUrl = "https://example.test/products/trailblazer",
            isActive = true
        });

        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<AdminRocketDto>(JsonOptions);

        Assert.NotNull(created);
        Assert.Equal("TestCo", created.Manufacturer);
        Assert.Equal("TL-100", created.Sku);

        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/admin/rockets/{created.Id}", new
        {
            manufacturer = "TestCo",
            sku = "TL-100",
            name = "Trailblazer Mk2",
            description = "Updated rocket",
            skillLevel = "Intermediate",
            recommendedEngines = new[] { "B6-4", "C6-5" },
            diameterMm = 25,
            lengthMm = 420,
            weightG = 72,
            finMaterial = "Plywood",
            noseCone = "Plastic",
            recoverySystem = "Parachute",
            thumbnailUrl = "https://example.test/rocket-thumb-2.jpg",
            imageUrls = new[] { "https://example.test/rocket-2.jpg" },
            productUrl = "https://example.test/products/trailblazer-mk2",
            isActive = true
        });

        updateResponse.EnsureSuccessStatusCode();
        var updated = await updateResponse.Content.ReadFromJsonAsync<AdminRocketDto>(JsonOptions);

        Assert.NotNull(updated);
        Assert.Equal("Trailblazer Mk2", updated.Name);
        Assert.Equal("Intermediate", updated.SkillLevel);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/admin/rockets/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var publicCatalogResponse = await _client.GetAsync($"/api/v1/rockets/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, publicCatalogResponse.StatusCode);

        var exportResponse = await _client.GetAsync("/api/v1/admin/seeds/export");
        exportResponse.EnsureSuccessStatusCode();

        var exportPayload = await exportResponse.Content.ReadFromJsonAsync<SeedExportPayload>(JsonOptions);
        Assert.NotNull(exportPayload);
        Assert.NotEmpty(exportPayload.Rockets);
        Assert.NotEmpty(exportPayload.Engines);
    }

    [Fact]
    public async Task AdminEngines_SupportsCreateUpdateDelete()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/admin/engines", new
        {
            manufacturer = "TestCo",
            designation = "B7-4",
            impulseClass = "B",
            totalImpulseNs = 5.2,
            averageThrustN = 7.1,
            delayS = 4,
            diameterMm = 18,
            lengthMm = 70,
            propellantWeightG = 2.9,
            totalWeightG = 18.8,
            caseType = "Single-use",
            propellantType = "Black powder",
            thumbnailUrl = "https://example.test/engine-thumb.jpg",
            imageUrls = new[] { "https://example.test/engine-1.jpg" },
            certificationBody = "NAR",
            isActive = true
        });

        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<AdminEngineDto>(JsonOptions);

        Assert.NotNull(created);
        Assert.Equal("B7-4", created.Designation);

        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/admin/engines/{created.Id}", new
        {
            manufacturer = "TestCo",
            designation = "B7-5",
            impulseClass = "B",
            totalImpulseNs = 5.4,
            averageThrustN = 7.3,
            delayS = 5,
            diameterMm = 18,
            lengthMm = 70,
            propellantWeightG = 3.0,
            totalWeightG = 19.0,
            caseType = "Single-use",
            propellantType = "Black powder",
            thumbnailUrl = "https://example.test/engine-thumb-2.jpg",
            imageUrls = new[] { "https://example.test/engine-2.jpg" },
            certificationBody = "NAR",
            isActive = true
        });

        updateResponse.EnsureSuccessStatusCode();
        var updated = await updateResponse.Content.ReadFromJsonAsync<AdminEngineDto>(JsonOptions);

        Assert.NotNull(updated);
        Assert.Equal("B7-5", updated.Designation);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/admin/engines/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var publicCatalogResponse = await _client.GetAsync($"/api/v1/engines/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, publicCatalogResponse.StatusCode);
    }

    [Fact]
    public async Task AdminImagesUpload_ReturnsStoredPath()
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("catalog-rockets"), "scope");

        var fileBytes = Encoding.UTF8.GetBytes("fake-admin-image");
        content.Add(new ByteArrayContent(fileBytes), "file", "catalog.jpg");

        var response = await _client.PostAsync("/api/v1/admin/images/upload", content);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<UploadImagePayload>(JsonOptions);
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.ImageUrl));
    }

    public sealed record AdminRocketDto(string Id, string Manufacturer, string Sku, string Name, string SkillLevel);

    public sealed record AdminEngineDto(string Id, string Designation);

    public sealed record UploadImagePayload(string ImageUrl);

    public sealed record SeedExportPayload(IReadOnlyList<object> Rockets, IReadOnlyList<object> Engines);
}
