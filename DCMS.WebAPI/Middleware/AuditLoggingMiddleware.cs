using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces;
using System.Security.Claims;

namespace DCMS.WebAPI.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork uow)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? string.Empty;

        // Only audit mutating operations
        var shouldAudit = method is "POST" or "PUT" or "DELETE" or "PATCH";

        await _next(context);

        if (!shouldAudit)
            return;

        try
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = context.User.FindFirstValue(ClaimTypes.Role);
            var statusCode = context.Response.StatusCode;

            var log = new SystemLog
            {
                UserId = userId != null ? int.Parse(userId) : null,
                UserRole = role ?? "Anonymous",
                ActionType = $"{method} {path}",
                HttpStatusCode = statusCode,
                IPAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                Date = DateTime.UtcNow,
                RetentionDate = DateTime.UtcNow.AddYears(2),
                EntityType = ExtractEntityType(path)
            };

            await uow.SystemLogs.AddAsync(log, CancellationToken.None);
            await uow.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Audit log write failed for {Method} {Path}", method, path);
        }
    }

    private static string ExtractEntityType(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length > 1 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase))
            return segments[1];
        return segments.Length > 0 ? segments[0] : "Unknown";
    }
}
