using DCMS.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DCMS.Infrastructure.Services;

/// <summary>
/// Reads the currently authenticated user's claims from HttpContext.
/// Registered as Scoped in DI — valid for the lifetime of a single HTTP request.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var claim = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User?.FindFirstValue("sub");
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? Email  => User?.FindFirstValue(ClaimTypes.Email);
    public string? Role   => User?.FindFirstValue(ClaimTypes.Role);
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
