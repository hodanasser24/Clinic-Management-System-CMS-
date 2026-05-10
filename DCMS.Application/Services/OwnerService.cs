using DCMS.Application.DTOs.Common;
using DCMS.Application.DTOs.Owner;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DCMS.Application.Services;

public class OwnerService : IOwnerService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly INotificationService _notificationService;

    public OwnerService(IUnitOfWork uow, IPasswordHasher<User> passwordHasher, INotificationService notificationService)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _notificationService = notificationService;
    }

    // ── Account Creation ──────────────────────────────────────────────────────

    public async Task<DoctorAccountResponseDto> CreateDoctorAccountAsync(CreateDoctorAccountRequestDto dto, CancellationToken ct = default)
    {
        var existing = await _uow.Doctors.GetByEmailAsync(dto.Email, ct);
        if (existing != null)
            throw new ConflictException($"An account with email '{dto.Email}' already exists.");

        var doctor = new Doctor
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Specialization = dto.Specialization,
            Qualification = dto.Qualification,
            Bio = dto.Bio,
            ExperienceYears = dto.ExperienceYears,
            Role = UserRole.Doctor,
            IsActive = true,
            IsFirstLogin = true
        };

        doctor.PasswordHash = _passwordHasher.HashPassword(doctor, dto.Password);

        await _uow.Doctors.AddAsync(doctor, ct);
        await _uow.SaveChangesAsync(ct);

        // Notify the new doctor
        await _notificationService.SendAsync(
            doctor.Id, NotificationType.AccountCreated, NotificationPriority.High,
            "Account Created", "Welcome! Your doctor account has been created. Please change your password on first login.",
            doctor.Id, "Doctor", ct);

        return MapToDoctorResponse(doctor);
    }

    public async Task<AccountResponseDto> CreateAdminAccountAsync(CreateAdminAccountRequestDto dto, CancellationToken ct = default)
    {
        var existing = await _uow.Admins.GetByEmailAsync(dto.Email, ct);
        if (existing != null)
            throw new ConflictException($"An account with email '{dto.Email}' already exists.");

        var admin = new Admin
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Role = UserRole.Admin,
            IsActive = true,
            IsFirstLogin = true
        };

        admin.PasswordHash = _passwordHasher.HashPassword(admin, dto.Password);

        await _uow.Admins.AddAsync(admin, ct);
        await _uow.SaveChangesAsync(ct);

        // Notify the new admin
        await _notificationService.SendAsync(
            admin.Id, NotificationType.AccountCreated, NotificationPriority.High,
            "Account Created", "Welcome! Your admin account has been created. Please change your password on first login.",
            admin.Id, "Admin", ct);

        return MapToAccountResponse(admin);
    }

    // ── Account Activation / Deactivation ─────────────────────────────────────

    public async Task<AccountResponseDto> DeactivateAccountAsync(int userId, DeactivateAccountRequestDto dto, CancellationToken ct = default)
    {
        var user = await GetUserByIdAsync(userId, ct);
        if (user == null) throw new NotFoundException($"User {userId} not found.");

        if (!user.IsActive)
            throw new BusinessRuleException("Account is already deactivated.");

        // Owner cannot deactivate themselves
        if (user.Role == UserRole.Owner)
            throw new ForbiddenException("Owner account cannot be deactivated.");

        user.IsActive = false;
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            user.Id, NotificationType.AccountDeactivated, NotificationPriority.High,
            "Account Deactivated", "Your account has been deactivated. Contact the clinic owner for more information.",
            user.Id, "User", ct);

        return MapToAccountResponse(user);
    }

    public async Task<AccountResponseDto> ReactivateAccountAsync(int userId, CancellationToken ct = default)
    {
        var user = await GetUserByIdAsync(userId, ct);
        if (user == null) throw new NotFoundException($"User {userId} not found.");

        if (user.IsActive)
            throw new BusinessRuleException("Account is already active.");

        user.IsActive = true;
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            user.Id, NotificationType.AccountCreated, NotificationPriority.Normal,
            "Account Reactivated", "Your account has been reactivated. You may now log in.",
            user.Id, "User", ct);

        return MapToAccountResponse(user);
    }

    // ── Doctor/Admin Listing ──────────────────────────────────────────────────

    public async Task<PagedResultDto<DoctorAccountResponseDto>> GetAllDoctorsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Doctors.GetPagedAsync(page, pageSize, ct: ct);
        return new PagedResultDto<DoctorAccountResponseDto>
        {
            Items = paged.Items.Select(MapToDoctorResponse).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<PagedResultDto<AccountResponseDto>> GetAllAdminsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Admins.GetPagedAsync(page, pageSize, ct: ct);
        return new PagedResultDto<AccountResponseDto>
        {
            Items = paged.Items.Select(MapToAccountResponse).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    // ── Offer Management — BR-58 ──────────────────────────────────────────────

    public async Task<OfferStatusResponseDto> ActivateOfferAsync(int offerId, CancellationToken ct = default)
    {
        var offer = await _uow.OfferDiscounts.GetByIdAsync(offerId, ct);
        if (offer == null) throw new NotFoundException($"Offer {offerId} not found.");

        if (offer.IsActive)
            throw new BusinessRuleException("Offer is already active.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (offer.EndDate < today)
            throw new BusinessRuleException("Cannot activate an expired offer.");

        offer.IsActive = true;
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendToRoleAsync(
            UserRole.Patient, NotificationType.OfferActivated, NotificationPriority.Normal,
            "New Offer Available", $"A new offer '{offer.Title}' is now active!", ct);

        return MapToOfferResponse(offer);
    }

    public async Task<OfferStatusResponseDto> DeactivateOfferAsync(int offerId, CancellationToken ct = default)
    {
        var offer = await _uow.OfferDiscounts.GetByIdAsync(offerId, ct);
        if (offer == null) throw new NotFoundException($"Offer {offerId} not found.");

        if (!offer.IsActive)
            throw new BusinessRuleException("Offer is already inactive.");

        offer.IsActive = false;
        await _uow.SaveChangesAsync(ct);

        return MapToOfferResponse(offer);
    }

    public async Task<PagedResultDto<OfferStatusResponseDto>> GetAllOffersAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.OfferDiscounts.GetPagedAsync(page, pageSize, ct: ct);
        return new PagedResultDto<OfferStatusResponseDto>
        {
            Items = paged.Items.Select(MapToOfferResponse).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    // ── Doctor Profile Management ─────────────────────────────────────────────

    public async Task<DoctorAccountResponseDto> UpdateDoctorProfileAsync(int doctorId, UpdateDoctorProfileRequestDto dto, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(doctorId, ct);
        if (doctor == null) throw new NotFoundException($"Doctor {doctorId} not found.");

        doctor.FullName = dto.FullName;
        doctor.Phone = dto.Phone;
        doctor.Specialization = dto.Specialization;
        doctor.Qualification = dto.Qualification;
        doctor.Bio = dto.Bio;
        doctor.ExperienceYears = dto.ExperienceYears;

        await _uow.SaveChangesAsync(ct);
        return MapToDoctorResponse(doctor);
    }

    public async Task<DoctorAccountResponseDto> UpdateDoctorPhotoAsync(int doctorId, UpdateDoctorPhotoRequestDto dto, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(doctorId, ct);
        if (doctor == null) throw new NotFoundException($"Doctor {doctorId} not found.");

        doctor.PhotoUrl = dto.PhotoUrl;
        await _uow.SaveChangesAsync(ct);
        return MapToDoctorResponse(doctor);
    }

    // ── Schedule change request approvals — BR-6 ──────────────────────────────

    public async Task<PagedResultDto<ScheduleChangeRequestResponseDto>> GetPendingScheduleRequestsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.ScheduleChangeRequests.GetPagedAsync(page, pageSize, r => r.Status == RequestStatus.Pending, ct);
        return new PagedResultDto<ScheduleChangeRequestResponseDto>
        {
            Items = paged.Items.Select(MapToScheduleChangeRequestResponse).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<ScheduleChangeRequestResponseDto> ApproveScheduleRequestAsync(int requestId, int ownerId, CancellationToken ct = default)
    {
        var request = await _uow.ScheduleChangeRequests.GetByIdAsync(requestId, ct);
        if (request == null) throw new NotFoundException($"Schedule change request {requestId} not found.");

        if (request.OwnerId != ownerId) throw new ForbiddenException("Only the designated Owner can approve this request.");
        if (request.Status != RequestStatus.Pending) throw new BusinessRuleException("Request is no longer pending.");

        request.Status = RequestStatus.Approved;
        request.OwnerApproved = true;

        // Apply changes to the actual schedule
        var schedule = await _uow.Schedules.GetByIdAsync(request.ScheduleId, ct);
        if (schedule != null)
        {
            if (request.ProposedDayOfWeek.HasValue) schedule.DayOfWeek = request.ProposedDayOfWeek.Value;
            if (request.ProposedStartTime.HasValue) schedule.StartTime = request.ProposedStartTime.Value;
            if (request.ProposedEndTime.HasValue) schedule.EndTime = request.ProposedEndTime.Value;
            if (request.ProposedSessionDurationMinutes.HasValue) schedule.SessionDurationMinutes = request.ProposedSessionDurationMinutes.Value;
        }

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            request.RequestingDoctorId, NotificationType.ScheduleChangeRequestApproved, NotificationPriority.Normal,
            "Schedule Request Approved", "Your schedule change request has been approved and applied.",
            request.Id, "ScheduleChangeRequest", ct);

        return MapToScheduleChangeRequestResponse(request);
    }

    public async Task<ScheduleChangeRequestResponseDto> RejectScheduleRequestAsync(int requestId, int ownerId, string? reason, CancellationToken ct = default)
    {
        var request = await _uow.ScheduleChangeRequests.GetByIdAsync(requestId, ct);
        if (request == null) throw new NotFoundException($"Schedule change request {requestId} not found.");

        if (request.OwnerId != ownerId) throw new ForbiddenException("Only the designated Owner can reject this request.");
        if (request.Status != RequestStatus.Pending) throw new BusinessRuleException("Request is no longer pending.");

        request.Status = RequestStatus.Rejected;
        request.Reason = reason ?? request.Reason; // Allow overriding reason during rejection

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            request.RequestingDoctorId, NotificationType.ScheduleChangeRequestRejected, NotificationPriority.Normal,
            "Schedule Request Rejected", $"Your schedule change request was rejected. Reason: {reason}",
            request.Id, "ScheduleChangeRequest", ct);

        return MapToScheduleChangeRequestResponse(request);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<User?> GetUserByIdAsync(int userId, CancellationToken ct)
    {
        var patient = await _uow.Patients.GetByIdAsync(userId, ct);
        if (patient != null) return patient;
        var doctor = await _uow.Doctors.GetByIdAsync(userId, ct);
        if (doctor != null) return doctor;
        return await _uow.Admins.GetByIdAsync(userId, ct);
    }

    private static ScheduleChangeRequestResponseDto MapToScheduleChangeRequestResponse(ScheduleChangeRequest r) => new()
    {
        Id = r.Id,
        RequestingDoctorId = r.RequestingDoctorId,
        RequestingDoctorName = r.Doctor?.FullName ?? "Unknown",
        ScheduleId = r.ScheduleId,
        ProposedDayOfWeek = r.ProposedDayOfWeek,
        ProposedStartTime = r.ProposedStartTime,
        ProposedEndTime = r.ProposedEndTime,
        ProposedSessionDurationMinutes = r.ProposedSessionDurationMinutes,
        Reason = r.Reason,
        DoctorApproved = r.DoctorApproved,
        OwnerApproved = r.OwnerApproved,
        Status = r.Status,
        ExpiresAt = r.ExpiresAt,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };

    private static DoctorAccountResponseDto MapToDoctorResponse(Doctor d) => new()
    {
        Id = d.Id,
        FullName = d.FullName,
        Email = d.Email,
        Phone = d.Phone,
        Role = d.Role,
        IsActive = d.IsActive,
        IsFirstLogin = d.IsFirstLogin,
        Specialization = d.Specialization,
        Qualification = d.Qualification,
        Bio = d.Bio,
        PhotoUrl = d.PhotoUrl,
        ExperienceYears = d.ExperienceYears,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
    };

    private static AccountResponseDto MapToAccountResponse(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        Phone = u.Phone,
        Role = u.Role,
        IsActive = u.IsActive,
        IsFirstLogin = u.IsFirstLogin,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt
    };

    private static OfferStatusResponseDto MapToOfferResponse(OfferDiscount o) => new()
    {
        Id = o.Id,
        Title = o.Title,
        IsActive = o.IsActive,
        StartDate = o.StartDate,
        EndDate = o.EndDate,
        BranchName = o.Branch?.Name ?? string.Empty,
        DiscountPercentage = o.DiscountPercentage
    };
}
