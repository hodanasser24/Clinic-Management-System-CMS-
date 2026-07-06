using DCMS.Application.DTOs.Auth;
using DCMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DCMS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private const string GuestCookieName = "dcms_guest_session";

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── Public endpoints ───────────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterPatientRequestDto dto, CancellationToken ct)
    {
        await _authService.RegisterPatientAsync(dto, ct);

        // BR-52: immediately invalidate guest session cookie after registration
        DeleteGuestSessionCookie();

        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(dto, ct);

        // BR-52: immediately invalidate guest session cookie after successful login
        DeleteGuestSessionCookie();

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequestDto dto, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(dto, ct);
        return Ok(result);
    }

    /// <summary>
    /// Step 1 of password reset flow.
    /// Generates a time-limited token and emails it to the user.
    /// Always returns 204 regardless of whether the email exists (prevents enumeration).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequestDto dto, CancellationToken ct)
    {
        await _authService.ForgotPasswordAsync(dto, ct);
        return NoContent();
    }

    /// <summary>
    /// Step 2 of password reset flow.
    /// Validates the token received by email and applies the new password.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequestDto dto, CancellationToken ct)
    {
        await _authService.ResetPasswordAsync(dto, ct);
        return NoContent();
    }

    // ── Authenticated endpoints ────────────────────────────────────────────────

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _authService.LogoutAsync(GetUserId(), ct);
        return NoContent();
    }

    /// <summary>
    /// BR-19: Change password — required on first login before any other action.
    /// Clears the IsFirstLogin flag upon success.
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequestDto dto, CancellationToken ct)
    {
        await _authService.ChangePasswordAsync(GetUserId(), dto, ct);
        return NoContent();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>
    /// BR-52: Expires the guest session cookie immediately by setting MaxAge to zero.
    /// The GuestSessionMiddleware will not create a new session for authenticated requests.
    /// </summary>
    private void DeleteGuestSessionCookie()
    {
        if (Request.Cookies.ContainsKey(GuestCookieName))
        {
            Response.Cookies.Append(GuestCookieName, string.Empty, new CookieOptions
            {
                HttpOnly  = true,
                Secure    = true,
                SameSite  = SameSiteMode.Strict,
                MaxAge    = TimeSpan.Zero   // immediate deletion
            });
        }
    }
}
