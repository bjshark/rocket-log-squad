using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace RocketLog.Api.IntegrationTests;

public sealed class LaunchEndpointsTests : IClassFixture<CatalogApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;

    public LaunchEndpointsTests(CatalogApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Launches_SupportCreateListDetailUpdatePhotoAndDelete()
    {
        var rocketCatalog = await _client.GetFromJsonAsync<PagedResponse<RocketCatalogItem>>(
            "/api/v1/rockets?page=1&pageSize=1",
            JsonOptions);

        Assert.NotNull(rocketCatalog);
        var rocket = Assert.Single(rocketCatalog.Items);

        var addRocketResponse = await _client.PostAsJsonAsync("/api/v1/my/rockets", new { rocketId = rocket.Id });
        addRocketResponse.EnsureSuccessStatusCode();

        var addRocketPayload = await addRocketResponse.Content.ReadFromJsonAsync<AddRocketPayload>(JsonOptions);
        Assert.NotNull(addRocketPayload);

        var engineCatalog = await _client.GetFromJsonAsync<PagedResponse<EngineCatalogItem>>(
            "/api/v1/engines?page=1&pageSize=1",
            JsonOptions);

        Assert.NotNull(engineCatalog);
        var engine = Assert.Single(engineCatalog.Items);

        var weatherResponse = await _client.GetAsync("/api/v1/weather?lat=40.01499&lng=-105.27055");
        weatherResponse.EnsureSuccessStatusCode();

        var weather = await weatherResponse.Content.ReadFromJsonAsync<WeatherPayload>(JsonOptions);
        Assert.NotNull(weather);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/my/launches", new
        {
            userRocketId = addRocketPayload.UserRocketId,
            engineId = engine.Id,
            launchDate = "2026-04-04T17:45:00Z",
            location = new
            {
                name = weather.LocationName,
                lat = 40.01499,
                lng = -105.27055
            },
            weather = new
            {
                source = weather.Source,
                temperatureF = weather.TemperatureF,
                windSpeedMph = weather.WindSpeedMph,
                windDirection = weather.WindDirection,
                humidity = weather.Humidity,
                conditions = weather.Conditions,
                visibilityMi = weather.VisibilityMi
            },
            outcome = "Success",
            altitudeFt = 512.5,
            notes = "Stable flight.",
            photoUrl = "https://example.test/launch.jpg"
        });

        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<LaunchDetailPayload>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal("Success", created.Outcome);

        var listResponse = await _client.GetAsync("/api/v1/my/launches?page=1&pageSize=10");
        listResponse.EnsureSuccessStatusCode();

        var listPayload = await listResponse.Content.ReadFromJsonAsync<PagedResponse<LaunchListItemPayload>>(JsonOptions);
        Assert.NotNull(listPayload);
        var listed = Assert.Single(listPayload.Items);
        Assert.Equal(created.Id, listed.Id);

        var detailResponse = await _client.GetAsync($"/api/v1/my/launches/{created.Id}");
        detailResponse.EnsureSuccessStatusCode();

        var detailPayload = await detailResponse.Content.ReadFromJsonAsync<LaunchDetailPayload>(JsonOptions);
        Assert.NotNull(detailPayload);
        Assert.Equal("Success", detailPayload.Outcome);

        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/my/launches/{created.Id}", new
        {
            userRocketId = addRocketPayload.UserRocketId,
            engineId = engine.Id,
            launchDate = "2026-04-04T18:00:00Z",
            location = new
            {
                name = "Updated launch site",
                lat = 40.012,
                lng = -105.265
            },
            weather = new
            {
                source = "manual",
                temperatureF = 72,
                windSpeedMph = 8,
                windDirection = "NW",
                humidity = 45,
                conditions = "Partly Cloudy",
                visibilityMi = 12
            },
            outcome = "Partial",
            altitudeFt = 410,
            notes = "Chute deployed late.",
            photoUrl = "https://example.test/launch-updated.jpg"
        });

        updateResponse.EnsureSuccessStatusCode();

        var updated = await updateResponse.Content.ReadFromJsonAsync<LaunchDetailPayload>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal("Partial", updated.Outcome);
        Assert.Equal("Updated launch site", updated.Location.Name);

        using var content = new MultipartFormDataContent();
        var fileBytes = Encoding.UTF8.GetBytes("fake-launch-image");
        content.Add(new ByteArrayContent(fileBytes), "file", "launch-photo.jpg");

        var uploadResponse = await _client.PostAsync($"/api/v1/my/launches/{created.Id}/photo", content);
        uploadResponse.EnsureSuccessStatusCode();

        var uploadPayload = await uploadResponse.Content.ReadFromJsonAsync<UploadPhotoPayload>(JsonOptions);
        Assert.NotNull(uploadPayload);
        Assert.Equal(created.Id, uploadPayload.LaunchId);
        Assert.False(string.IsNullOrWhiteSpace(uploadPayload.PhotoUrl));

        var deleteResponse = await _client.DeleteAsync($"/api/v1/my/launches/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var finalList = await _client.GetFromJsonAsync<PagedResponse<LaunchListItemPayload>>(
            "/api/v1/my/launches?page=1&pageSize=10",
            JsonOptions);

        Assert.NotNull(finalList);
        Assert.Empty(finalList.Items);
    }

    public sealed record PagedResponse<T>(IReadOnlyList<T> Items, long Total, int Page, int PageSize);

    public sealed record RocketCatalogItem(string Id);

    public sealed record EngineCatalogItem(string Id);

    public sealed record AddRocketPayload(string UserRocketId, string RocketId, bool Added);

    public sealed record WeatherPayload(
        string Source,
        double TemperatureF,
        double WindSpeedMph,
        string WindDirection,
        double Humidity,
        string Conditions,
        double? VisibilityMi,
        string? LocationName);

    public sealed record LaunchListItemPayload(string Id);

    public sealed record LaunchDetailPayload(string Id, string Outcome, LaunchLocationPayload Location);

    public sealed record LaunchLocationPayload(string? Name, double Lat, double Lng);

    public sealed record UploadPhotoPayload(string LaunchId, string? PhotoUrl);
}
