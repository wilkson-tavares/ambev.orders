using Orders.Domain.Exceptions;
using System.Text.Json;

namespace Orders.API.Middlewares;

public sealed class ExceptionMiddleware
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
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain error: {Message}", ex.Message);

            var isDuplicidade = ex.Message.Contains("already exists");

            context.Response.StatusCode = isDuplicidade
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;

            context.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(new { error = ex.Message });
            await context.Response.WriteAsync(body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}