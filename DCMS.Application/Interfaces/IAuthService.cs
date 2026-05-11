using DCMS.Application.DTOs.Auth;

namespace DCMS.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto, CancellationToken ct = default);
    Task RegisterPatientAsync(RegisterPatientRequestDto dto, CancellationToken ct = default);
    Task ChangePasswordAsync(int userId, ChangePasswordRequestDto dto, CancellationToken ct = default);

    /// <summary>
    /// Step 1 of password reset: generates a secure time-limited token, stores its hash
    /// on the user record, and emails the plaintext token to the user.
    /// </summary>
    Task ForgotPasswordAsync(ForgotPasswordRequestDto dto, CancellationToken ct = default);

    /// <summary>
    /// Step 2 of password reset: validates the token, resets the password, and clears
    /// the reset token. Throws BusinessRuleException if the token is invalid or expired.
    /// </summary>
    Task ResetPasswordAsync(ResetPasswordRequestDto dto, CancellationToken ct = default);

    Task LogoutAsync(int userId, CancellationToken ct = default);
}
