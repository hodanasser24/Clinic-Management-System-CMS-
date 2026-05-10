using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace DCMS.WebAPI.Middleware;

/// <summary>
/// BR-19: If a user's IsFirstLogin claim is true, they must change their password
/// before accessing any other endpoint. Only /api/auth/change-password is permitted.
/// </summary>
public class FirstLoginMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly HashSet<string> AllowedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/change-password",
        "/api/auth/logout",
        "/api/auth/refresh"
    };

    public FirstLoginMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Only applies to authenticated users
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var isFirstLogin = context.User.FindFirstValue("isFirstLogin");
            if (string.Equals(isFirstLogin, "True", StringComparison.OrdinalIgnoreCase))
            {
                var path = context.Request.Path.Value ?? string.Empty;
                var isAllowed = AllowedPaths.Any(p =>
                    path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

                if (!isAllowed)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    context.Response.ContentType = "application/json";
                    var body = JsonSerializer.Serialize(new
                    {
                        status = 403,
                        title = "Password Change Required",
                        detail = "You must change your temporary password before proceeding. Use POST /api/auth/change-password.",
                        traceId = context.TraceIdentifier
                    }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    await context.Response.WriteAsync(body);
                    return;
                }
            }
        }

        await _next(context);
    }
}
