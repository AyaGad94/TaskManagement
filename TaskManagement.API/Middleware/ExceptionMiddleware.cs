using System.Text.Json;
using TaskManagement.BLL.Exceptions;

namespace TaskManagement.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (BadRequestException ex)
        {
            _logger.LogWarning(ex, "Bad request: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            var response = new { error = ex.Message, detail = ex.InnerException?.Message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            var response = new { error = "An unexpected error occurred.", detail = ex.InnerException?.Message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}