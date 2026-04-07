namespace RocketLog.Api.Services;

public interface IWeatherService
{
    Task<bool> IsConfiguredAsync(CancellationToken cancellationToken = default);

    Task<WeatherSnapshot> GetSnapshotAsync(double lat, double lng, CancellationToken cancellationToken = default);
}

public sealed record WeatherSnapshot(
    string Source,
    double TemperatureF,
    double WindSpeedMph,
    string WindDirection,
    double Humidity,
    string Conditions,
    double? VisibilityMi,
    string? LocationName);