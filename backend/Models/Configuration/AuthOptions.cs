namespace RocketLog.Api.Models.Configuration;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public bool Enabled { get; set; } = true;

    public DevUserOptions DevUser { get; set; } = new();

    public JwtOptions Jwt { get; set; } = new();
}

public sealed class DevUserOptions
{
    public string Subject { get; set; } = "dev-admin";

    public string Email { get; set; } = "dev-admin@rocketlog.local";

    public string DisplayName { get; set; } = "Rocket Log Dev Admin";

    public string[] Roles { get; set; } = ["admin", "user"];
}

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "RocketLog.Api";

    public string Audience { get; set; } = "RocketLog.Client";

    public string SigningKey { get; set; } = "rocket-log-dev-signing-key-please-change-before-production";
}