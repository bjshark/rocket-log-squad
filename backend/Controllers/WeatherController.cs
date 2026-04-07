using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RocketLog.Api.Models.Common;
using RocketLog.Api.Services;

namespace RocketLog.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/weather")]
public sealed class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWeather(
        [FromQuery] double? lat,
        [FromQuery] double? lng,
        CancellationToken cancellationToken = default)
    {
        if (lat is null || lng is null)
        {
            return BadRequest(new ApiErrorResponse("Both latitude and longitude are required.", "LatLngRequired"));
        }

        if (lat < -90 || lat > 90)
        {
            return BadRequest(new ApiErrorResponse("Latitude must be between -90 and 90.", "InvalidLatitude"));
        }

        if (lng < -180 || lng > 180)
        {
            return BadRequest(new ApiErrorResponse("Longitude must be between -180 and 180.", "InvalidLongitude"));
        }

        var snapshot = await _weatherService.GetSnapshotAsync(lat.Value, lng.Value, cancellationToken);

        return Ok(new WeatherSnapshotDto(
            snapshot.Source,
            snapshot.TemperatureF,
            snapshot.WindSpeedMph,
            snapshot.WindDirection,
            snapshot.Humidity,
            snapshot.Conditions,
            snapshot.VisibilityMi,
            snapshot.LocationName));
    }

    public sealed record WeatherSnapshotDto(
        string Source,
        double TemperatureF,
        double WindSpeedMph,
        string WindDirection,
        double Humidity,
        string Conditions,
        double? VisibilityMi,
        string? LocationName);
}
