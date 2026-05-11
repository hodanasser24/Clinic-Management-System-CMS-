using DCMS.Domain.Enums;

namespace DCMS.Application.DTOs.Auth;

public class LoginRequestDto
{
    public string Email    { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginResponseDto
{
    public string   AccessToken  { get; set; } = null!;
    public string   RefreshToken { get; set; } = null!;
    public int      UserId       { get; set; }
    public string   FullName     { get; set; } = null!;
    public string   Email        { get; set; } = null!;
    public UserRole Role         { get; set; }
    public bool     IsFirstLogin { get; set; }
}

public class RegisterPatientRequestDto
{
    public string  FullName       { get; set; } = null!;
    public string  Email          { get; set; } = null!;
    public string  Password       { get; set; } = null!;
    public string  Phone          { get; set; } = null!;
    public DateOnly DateOfBirth   { get; set; }
    public string? MedicalHistory { get; set; }
}

public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword     { get; set; } = null!;
}

/// <summary>
/// Step 1 of the password-reset flow.
/// The user provides their email; the system emails them a time-limited token.
/// </summary>
public class ForgotPasswordRequestDto
{
    public string Email { get; set; } = null!;
}

/// <summary>
/// Step 2 of the password-reset flow.
/// The user provides the token they received by email plus their new password.
/// </summary>
public class ResetPasswordRequestDto
{
    public string Email       { get; set; } = null!;
    public string Token       { get; set; } = null!;   // plaintext token from email
    public string NewPassword { get; set; } = null!;
}

public class RefreshTokenRequestDto
{
    public string AccessToken  { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}
