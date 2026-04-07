using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RocketLog.Api.Models.Configuration;

namespace RocketLog.Api.Controllers;

[ApiController]
[Route("api/v1/system")]
public sealed class SystemController : ControllerBase
{
    private readonly AuthOptions _authOptions;
    private readonly IWebHostEnvironment _environment;

    public SystemController(IOptions<AuthOptions> authOptions, IWebHostEnvironment environment)
    {
        _authOptions = authOptions.Value;
        _environment = environment;
    }

    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "ok",
            environment = _environment.EnvironmentName,
            authEnabled = _authOptions.Enabled,
            timestampUtc = DateTime.UtcNow
        });
    }
}