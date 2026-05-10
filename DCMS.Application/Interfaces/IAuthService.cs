using DCMS.Application.DTOs.Auth;

namespace DCMS.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto, CancellationToken ct = default);
    Task RegisterPatientAsync(RegisterPatientRequestDto dto, CancellationToken ct = default);
    Task ChangePasswordAsync(int userId, ChangePasswordRequestDto dto, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordRequestDto dto, CancellationToken ct = default);
    Task LogoutAsync(int userId, CancellationToken ct = default);
}
