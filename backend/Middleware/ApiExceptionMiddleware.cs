using System.Text.Json;
using MongoDB.Driver;
using RocketLog.Api.Models.Common;

namespace RocketLog.Api.Middleware;

public sealed class ApiExceptionMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (MongoException exception)
        {
            _logger.LogError(exception, "A MongoDB error occurred while handling {Path}.", context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status503ServiceUnavailable, "Database is unavailable.", "DatabaseUnavailable");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred while handling {Path}.", context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.", "ServerError");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string error, string code)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = new ApiErrorResponse(error, code);
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, SerializerOptions));
    }
}