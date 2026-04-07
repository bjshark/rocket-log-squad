using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using RocketLog.Api.Data;
using RocketLog.Api.Middleware;
using RocketLog.Api.Models.Configuration;
using RocketLog.Api.Seeders;
using RocketLog.Api.Services;

var builder = WebApplication.CreateBuilder(args);
const string LocalDevCorsPolicyName = "LocalDevCors";

var corsOrigins = ResolveCorsOrigins(builder.Configuration, builder.Environment.IsDevelopment());
var corsAllowCredentials = builder.Configuration.GetValue<bool>("Cors:AllowCredentials");

builder.Services.Configure<MongoDbOptions>(
    builder.Configuration.GetSection(MongoDbOptions.SectionName));
builder.Services.Configure<AuthOptions>(
    builder.Configuration.GetSection(AuthOptions.SectionName));

var authOptions = builder.Configuration
    .GetSection(AuthOptions.SectionName)
    .Get<AuthOptions>() ?? new AuthOptions();

var signingKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(authOptions.Jwt.SigningKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = authOptions.Jwt.Issuer,
            ValidAudience = authOptions.Jwt.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

if (corsOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(LocalDevCorsPolicyName, policy =>
        {
            policy.WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();

            if (corsAllowCredentials)
            {
                policy.AllowCredentials();
            }
        });
    });
}

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(builder.Configuration.GetConnectionString("MongoDb")
        ?? builder.Configuration[$"{MongoDbOptions.SectionName}:ConnectionString"]));
builder.Services.AddScoped<MongoDbContext>();
builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
builder.Services.AddScoped<IDataSeeder, MongoDataSeeder>();
builder.Services.AddScoped<IWeatherService, WeatherService>();

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
if (corsOrigins.Length > 0)
{
    app.UseCors(LocalDevCorsPolicyName);
}
app.UseMiddleware<DevAuthBypassMiddleware>();

if (app.Configuration.GetValue<bool>($"{AuthOptions.SectionName}:Enabled"))
{
    app.UseAuthentication();
}

app.UseAuthorization();
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.SeedAsync();
}

app.Run();

static string[] ResolveCorsOrigins(IConfiguration configuration, bool isDevelopment)
{
    var sectionOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    var rawOrigins = sectionOrigins?.Length > 0
        ? sectionOrigins
        : configuration["Cors:AllowedOrigins"]?.Split(',', ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? Array.Empty<string>();

    var normalizedOrigins = rawOrigins
        .Select(origin => origin.Trim().TrimEnd('/'))
        .Where(origin => !string.IsNullOrWhiteSpace(origin))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    if (normalizedOrigins.Length > 0)
    {
        return normalizedOrigins;
    }

    if (!isDevelopment)
    {
        return Array.Empty<string>();
    }

    var hosts = new[] { "localhost", "127.0.0.1" };
    var ports = Enumerable.Range(5173, 8).Concat(Enumerable.Range(4173, 8));

    return hosts
        .SelectMany(host => ports.SelectMany(port => new[]
        {
            $"http://{host}:{port}",
            $"https://{host}:{port}"
        }))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}

public partial class Program;
