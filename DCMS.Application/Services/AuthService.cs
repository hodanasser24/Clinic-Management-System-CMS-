using System.Security.Cryptography;
using DCMS.Application.DTOs.Auth;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DCMS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork              _uow;
    private readonly IPasswordHasher<User>    _passwordHasher;
    private readonly IJwtTokenService         _jwtTokenService;
    private readonly IEmailService            _emailService;
    private readonly INotificationService     _notificationService;

    // Reset token lifetime
    private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromHours(1);

    public AuthService(
        IUnitOfWork              uow,
        IPasswordHasher<User>    passwordHasher,
        IJwtTokenService         jwtTokenService,
        IEmailService            emailService,
        INotificationService     notificationService)
    {
        _uow                 = uow;
        _passwordHasher      = passwordHasher;
        _jwtTokenService     = jwtTokenService;
        _emailService        = emailService;
        _notificationService = notificationService;
    }

    // ── Login ─────────────────────────────────────────────────────────────

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var user = await FindUserByEmailAsync(dto.Email, ct);
        if (user == null)
            throw new NotFoundException("Invalid email or password.");
        if (!user.IsActive)
            throw new ForbiddenException("Account is deactivated. Contact the clinic owner.");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new NotFoundException("Invalid email or password.");

        var token        = _jwtTokenService.GenerateToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken           = refreshToken;
        user.RefreshTokenExpiresAt  = DateTime.UtcNow.AddDays(7);
        await _uow.SaveChangesAsync(ct);

        // BR-52: caller (AuthController) must delete the guest session cookie after this returns
        return BuildLoginResponse(user, token, refreshToken);
    }

    // ── Refresh ───────────────────────────────────────────────────────────

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto, CancellationToken ct = default)
    {
        var userId = _jwtTokenService.GetUserIdFromExpiredToken(dto.AccessToken);
        var user   = await GetUserByIdAsync(userId, ct);

        if (user == null
            || user.RefreshToken != dto.RefreshToken
            || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
        {
            throw new ForbiddenException("Invalid or expired refresh token.");
        }

        var newToken        = _jwtTokenService.GenerateToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken          = newRefreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        await _uow.SaveChangesAsync(ct);

        return BuildLoginResponse(user, newToken, newRefreshToken);
    }

    // ── Register ──────────────────────────────────────────────────────────

    public async Task RegisterPatientAsync(RegisterPatientRequestDto dto, CancellationToken ct = default)
    {
        var existing = await _uow.Patients.GetByEmailAsync(dto.Email, ct);
        if (existing != null)
            throw new ConflictException("Email is already registered.");

        var patient = new Patient
        {
            FullName       = dto.FullName,
            Email          = dto.Email,
            Phone          = dto.Phone,
            DateOfBirth    = dto.DateOfBirth,
            MedicalHistory = dto.MedicalHistory,
            Role           = UserRole.Patient,
            IsActive       = true,
            IsFirstLogin   = false   // Self-registered patients don't get forced first-login
        };
        patient.PasswordHash = _passwordHasher.HashPassword(patient, dto.Password);

        await _uow.Patients.AddAsync(patient, ct);
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            patient.Id, NotificationType.AccountCreated, NotificationPriority.Normal,
            "Welcome!", "Your patient account has been created successfully.",
            patient.Id, "Patient", ct);

        // BR-52: caller (AuthController) must delete the guest session cookie after this returns
    }

    // ── Change Password ───────────────────────────────────────────────────

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto dto, CancellationToken ct = default)
    {
        var user = await GetUserByIdAsync(userId, ct)
            ?? throw new NotFoundException("User not found.");

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
        if (verify == PasswordVerificationResult.Failed)
            throw new BusinessRuleException("Current password is incorrect.");

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
        user.IsFirstLogin = false;  // BR-19: clears first-login flag

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            userId, NotificationType.PasswordChanged, NotificationPriority.Normal,
            "Password Changed", "Your password has been updated successfully.", ct: ct);
    }

    // ── Forgot Password (Step 1) ──────────────────────────────────────────

    /// <summary>
    /// Generates a secure random token, stores its SHA-256 hash on the user record,
    /// and emails the plaintext token. Token is valid for 1 hour.
    /// Always returns success even if the email is not found (security best practice).
    /// </summary>
    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto dto, CancellationToken ct = default)
    {
        var user = await FindUserByEmailAsync(dto.Email, ct);
        if (user == null)
            return; // Do not reveal whether the email exists

        // Generate cryptographically secure plaintext token
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var plainToken = Convert.ToBase64String(tokenBytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('='); // URL-safe

        // Store the hash (never the plaintext token)
        user.PasswordResetTokenHash       = HashToken(plainToken);
        user.PasswordResetTokenExpiresAt  = DateTime.UtcNow.Add(ResetTokenLifetime);

        await _uow.SaveChangesAsync(ct);

        // Email plaintext token to user
        await _emailService.SendPasswordResetAsync(user.Email, user.FullName, plainToken, ct);
    }

    // ── Reset Password (Step 2) ───────────────────────────────────────────

    /// <summary>
    /// Validates the reset token, resets the password, and clears the token fields.
    /// Throws BusinessRuleException if the token is invalid, expired, or already used.
    /// </summary>
    public async Task ResetPasswordAsync(ResetPasswordRequestDto dto, CancellationToken ct = default)
    {
        var user = await FindUserByEmailAsync(dto.Email, ct)
            ?? throw new NotFoundException("Email not found.");

        if (string.IsNullOrEmpty(user.PasswordResetTokenHash)
            || user.PasswordResetTokenExpiresAt == null
            || user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
        {
            throw new BusinessRuleException("Password reset token has expired or was not requested. Use Forgot Password to generate a new one.");
        }

        var expectedHash = HashToken(dto.Token);
        if (!string.Equals(expectedHash, user.PasswordResetTokenHash, StringComparison.Ordinal))
            throw new BusinessRuleException("Invalid password reset token.");

        // Apply new password and clear token fields
        user.PasswordHash                = _passwordHasher.HashPassword(user, dto.NewPassword);
        user.IsFirstLogin                = false;
        user.PasswordResetTokenHash      = null;
        user.PasswordResetTokenExpiresAt = null;

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            user.Id, NotificationType.PasswordChanged, NotificationPriority.Normal,
            "Password Reset", "Your password has been reset successfully.", ct: ct);
    }

    // ── Logout ────────────────────────────────────────────────────────────

    public async Task LogoutAsync(int userId, CancellationToken ct = default)
    {
        var user = await GetUserByIdAsync(userId, ct);
        if (user == null) return;

        user.RefreshToken          = null;
        user.RefreshTokenExpiresAt = null;
        await _uow.SaveChangesAsync(ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task<User?> FindUserByEmailAsync(string email, CancellationToken ct)
    {
        User? user = await _uow.Patients.GetByEmailAsync(email, ct);
        if (user != null) return user;
        user = await _uow.Doctors.GetByEmailAsync(email, ct);
        if (user != null) return user;
        return await _uow.Admins.GetByEmailAsync(email, ct);
    }

    private async Task<User?> GetUserByIdAsync(int userId, CancellationToken ct)
    {
        User? user = await _uow.Patients.GetByIdAsync(userId, ct);
        if (user != null) return user;
        user = await _uow.Doctors.GetByIdAsync(userId, ct);
        if (user != null) return user;
        return await _uow.Admins.GetByIdAsync(userId, ct);
    }

    private static string HashToken(string plainToken)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(plainToken);
        var hash  = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private static LoginResponseDto BuildLoginResponse(User user, string token, string refresh) => new()
    {
        AccessToken  = token,
        RefreshToken = refresh,
        UserId       = user.Id,
        FullName     = user.FullName,
        Email        = user.Email,
        Role         = user.Role,
        IsFirstLogin = user.IsFirstLogin
    };
}
