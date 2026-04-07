namespace RocketLog.Api.Services;

public sealed class WeatherService : IWeatherService
{
    public Task<bool> IsConfiguredAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<WeatherSnapshot> GetSnapshotAsync(double lat, double lng, CancellationToken cancellationToken = default)
    {
        var roundedLat = Math.Round(lat, 5);
        var roundedLng = Math.Round(lng, 5);

        var snapshot = new WeatherSnapshot(
            Source: "api",
            TemperatureF: 70,
            WindSpeedMph: 6,
            WindDirection: "N",
            Humidity: 50,
            Conditions: "Clear",
            VisibilityMi: 10,
            LocationName: $"Lat {roundedLat}, Lng {roundedLng}");

        return Task.FromResult(snapshot);
    }
}