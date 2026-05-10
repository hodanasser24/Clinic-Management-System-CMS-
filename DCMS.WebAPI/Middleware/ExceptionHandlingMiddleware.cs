using DCMS.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace DCMS.WebAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Not Found"),
            ForbiddenException => (HttpStatusCode.Forbidden, "Forbidden"),
            ConflictException => (HttpStatusCode.Conflict, "Conflict"),
            ValidationException => (HttpStatusCode.BadRequest, "Validation Error"),
            BusinessRuleException => ((HttpStatusCode)422, "Unprocessable Entity"),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var response = new
        {
            status = (int)statusCode,
            title,
            detail = exception.Message,
            traceId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
