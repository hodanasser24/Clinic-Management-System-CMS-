using DCMS.Domain.Enums;

namespace DCMS.Application.DTOs.Auth;

public class LoginRequestDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginResponseDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public UserRole Role { get; set; }
    public bool IsFirstLogin { get; set; }
}

public class RegisterPatientRequestDto
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public string? MedicalHistory { get; set; }
}

public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}

public class ResetPasswordRequestDto
{
    public string Email { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string Token { get; set; } = null!;
}

public class RefreshTokenRequestDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}
