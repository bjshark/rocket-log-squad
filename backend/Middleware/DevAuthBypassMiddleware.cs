using System.Security.Claims;
using Microsoft.Extensions.Options;
using RocketLog.Api.Models.Configuration;

namespace RocketLog.Api.Middleware;

public sealed class DevAuthBypassMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AuthOptions _authOptions;

    public DevAuthBypassMiddleware(RequestDelegate next, IOptions<AuthOptions> authOptions)
    {
        _next = next;
        _authOptions = authOptions.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_authOptions.Enabled && !(context.User.Identity?.IsAuthenticated ?? false))
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, _authOptions.DevUser.Subject),
                new(ClaimTypes.Email, _authOptions.DevUser.Email),
                new(ClaimTypes.Name, _authOptions.DevUser.DisplayName)
            };

            claims.AddRange(_authOptions.DevUser.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            context.User = new ClaimsPrincipal(
                new ClaimsIdentity(claims, authenticationType: "DevelopmentBypass"));
        }

        await _next(context);
    }
}