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
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public AuthService(
        IUnitOfWork uow,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        INotificationService notificationService)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var user = await FindUserByEmailAsync(dto.Email, ct);
        if (user == null) throw new NotFoundException("Invalid email or password.");
        if (!user.IsActive) throw new ForbiddenException("Account is deactivated.");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new NotFoundException("Invalid email or password.");

        var token = _jwtTokenService.GenerateToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        await _uow.SaveChangesAsync(ct);

        return BuildLoginResponse(user, token, refreshToken);
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto, CancellationToken ct = default)
    {
        var userId = _jwtTokenService.GetUserIdFromExpiredToken(dto.AccessToken);
        var user = await GetUserByIdAsync(userId, ct);

        if (user == null || user.RefreshToken != dto.RefreshToken || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
            throw new ForbiddenException("Invalid or expired refresh token.");

        var newToken = _jwtTokenService.GenerateToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        await _uow.SaveChangesAsync(ct);

        return BuildLoginResponse(user, newToken, newRefreshToken);
    }

    public async Task RegisterPatientAsync(RegisterPatientRequestDto dto, CancellationToken ct = default)
    {
        var existing = await _uow.Patients.GetByEmailAsync(dto.Email, ct);
        if (existing != null) throw new ConflictException("Email is already registered.");

        var patient = new Patient
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            MedicalHistory = dto.MedicalHistory,
            Role = UserRole.Patient,
            IsActive = true,
            IsFirstLogin = false // Patients self-register — no forced first-login
        };
        patient.PasswordHash = _passwordHasher.HashPassword(patient, dto.Password);

        await _uow.Patients.AddAsync(patient, ct);
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            patient.Id, NotificationType.AccountCreated, NotificationPriority.Normal,
            "Welcome!", "Your patient account has been created successfully.",
            patient.Id, "Patient", ct);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto dto, CancellationToken ct = default)
    {
        var user = await GetUserByIdAsync(userId, ct);
        if (user == null) throw new NotFoundException("User not found.");

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
        if (verify == PasswordVerificationResult.Failed)
            throw new BusinessRuleException("Current password is incorrect.");

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
        user.IsFirstLogin = false; // BR-19: clear first-login flag after password change

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            userId, NotificationType.PasswordChanged, NotificationPriority.Normal,
            "Password Changed", "Your password has been changed successfully.", ct: ct);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto dto, CancellationToken ct = default)
    {
        var user = await FindUserByEmailAsync(dto.Email, ct);
        if (user == null) throw new NotFoundException("Email not found.");

        var tempPassword = Guid.NewGuid().ToString("N")[..12];
        user.PasswordHash = _passwordHasher.HashPassword(user, tempPassword);
        user.IsFirstLogin = true; // Force password change on next login
        await _uow.SaveChangesAsync(ct);

        await _emailService.SendPasswordResetAsync(user.Email, user.FullName, tempPassword, ct);
    }

    public async Task LogoutAsync(int userId, CancellationToken ct = default)
    {
        var user = await GetUserByIdAsync(userId, ct);
        if (user == null) return;

        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        await _uow.SaveChangesAsync(ct);
    }

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

    private static LoginResponseDto BuildLoginResponse(User user, string token, string refresh) => new()
    {
        AccessToken = token,
        RefreshToken = refresh,
        UserId = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role,
        IsFirstLogin = user.IsFirstLogin
    };
}
