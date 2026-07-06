using DCMS.Domain.Interfaces;
using DCMS.Domain.Entities;

namespace DCMS.WebAPI.Middleware;

/// <summary>
/// BR-50: Assigns a persistent session-based identity (Guid) to unauthenticated (Guest) requests.
/// BR-51: Session cookie expires after 30 minutes of inactivity (sliding expiration).
/// </summary>
public class GuestSessionMiddleware
{
    private const string CookieName = "dcms_guest_session";
    // BR-51: 30 minutes inactivity timeout
    private static readonly TimeSpan SessionTimeout = TimeSpan.FromMinutes(30);
    private readonly RequestDelegate _next;

    public GuestSessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork uow)
    {
        // Only apply to unauthenticated requests
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            string? sessionId = null;

            if (context.Request.Cookies.TryGetValue(CookieName, out var cookieValue)
                && Guid.TryParse(cookieValue, out _))
            {
                sessionId = cookieValue;

                // BR-51: Sliding expiration — refresh cookie on every request
                context.Response.Cookies.Append(CookieName, sessionId, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30) // BR-51: 30 minutes
                });
            }
            else
            {
                sessionId = Guid.NewGuid().ToString();

                context.Response.Cookies.Append(CookieName, sessionId, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30) // BR-51: 30 minutes
                });

                var existingGuest = await uow.Guests.GetBySessionIdAsync(sessionId, CancellationToken.None);
                if (existingGuest == null)
                {
                    var guest = new Guest
                    {
                        SessionId = sessionId,
                        FullName = "Guest",
                        Email = $"guest_{sessionId[..8]}@dcms.local",
                        PasswordHash = string.Empty,
                        Role = DCMS.Domain.Enums.UserRole.Guest,
                        IsActive = true
                    };
                    await uow.Guests.AddAsync(guest, CancellationToken.None);
                    await uow.SaveChangesAsync(CancellationToken.None);
                }
            }

            context.Items["GuestSessionId"] = sessionId;
        }

        await _next(context);
    }
}
