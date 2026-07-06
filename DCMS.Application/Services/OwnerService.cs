using DCMS.Application.DTOs.Common;
using DCMS.Application.DTOs.Owner;
using DCMS.Application.DTOs.Schedules;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

using AutoMapper;

namespace DCMS.Application.Services;

public class OwnerService : IOwnerService
{
    private readonly IUnitOfWork          _uow;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly INotificationService _notificationService;
    private readonly IMapper              _mapper;

    public OwnerService(
        IUnitOfWork          uow,
        IPasswordHasher<User> passwordHasher,
        INotificationService notificationService,
        IMapper              mapper)
    {
        _uow                 = uow;
        _passwordHasher      = passwordHasher;
        _notificationService = notificationService;
        _mapper              = mapper;
    }

    // ── Account Creation ──────────────────────────────────────────────────────

    public async Task<DoctorAccountResponseDto> CreateDoctorAccountAsync(
        CreateDoctorAccountRequestDto dto, CancellationToken ct = default)
    {
        var existing = await _uow.Doctors.GetByEmailAsync(dto.Email, ct);
        if (existing != null)
            throw new ConflictException($"An account with email '{dto.Email}' already exists.");

        var doctor = new Doctor
        {
            FullName       = dto.FullName,
            Email          = dto.Email,
            Phone          = dto.Phone,
            Specialization = dto.Specialization,
            Qualification  = dto.Qualification,
            Bio            = dto.Bio,
            ExperienceYears = dto.ExperienceYears,
            Role           = UserRole.Doctor,
            IsActive       = true,
            IsFirstLogin   = true   // Must change password on first login (BR-19)
        };
        doctor.PasswordHash = _passwordHasher.HashPassword(doctor, dto.Password);

        await _uow.Doctors.AddAsync(doctor, ct);
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            doctor.Id, NotificationType.AccountCreated, NotificationPriority.High,
            "Welcome to DCMS",
            "Your doctor account has been created. Please change your temporary password on first login.",
            doctor.Id, "Doctor", ct);

        return _mapper.Map<DoctorAccountResponseDto>(doctor);
    }

    public async Task<AccountResponseDto> CreateAdminAccountAsync(
        CreateAdminAccountRequestDto dto, CancellationToken ct = default)
    {
        var existing = await _uow.Admins.GetByEmailAsync(dto.Email, ct);
        if (existing != null)
            throw new ConflictException($"An account with email '{dto.Email}' already exists.");

        var admin = new Admin
        {
            FullName     = dto.FullName,
            Email        = dto.Email,
            Phone        = dto.Phone,
            Role         = UserRole.Admin,
            IsActive     = true,
            IsFirstLogin = true
        };
        admin.PasswordHash = _passwordHasher.HashPassword(admin, dto.Password);

        await _uow.Admins.AddAsync(admin, ct);
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            admin.Id, NotificationType.AccountCreated, NotificationPriority.High,
            "Welcome to DCMS",
            "Your admin account has been created. Please change your temporary password on first login.",
            admin.Id, "Admin", ct);

        return _mapper.Map<AccountResponseDto>(admin);
    }

    // ── Account Activation / Deactivation ─────────────────────────────────────

    public async Task<AccountResponseDto> DeactivateAccountAsync(
        int userId, DeactivateAccountRequestDto dto, CancellationToken ct = default)
    {
        var user = await GetUserByIdAsync(userId, ct)
            ?? throw new NotFoundException($"User {userId} not found.");

        if (!user.IsActive)
            throw new BusinessRuleException("Account is already deactivated.");

        if (user.Role == UserRole.Owner)
            throw new ForbiddenException("The Owner account cannot be deactivated.");

        user.IsActive = false;
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            user.Id, NotificationType.AccountDeactivated, NotificationPriority.High,
            "Account Deactivated",
            "Your account has been deactivated. Contact the clinic owner for more information.",
            user.Id, "User", ct);

        return _mapper.Map<AccountResponseDto>(user);
    }

    public async Task<AccountResponseDto> ReactivateAccountAsync(
        int userId, CancellationToken ct = default)
    {
        var user = await GetUserByIdAsync(userId, ct)
            ?? throw new NotFoundException($"User {userId} not found.");

        if (user.IsActive)
            throw new BusinessRuleException("Account is already active.");

        user.IsActive = true;
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            user.Id, NotificationType.AccountCreated, NotificationPriority.Normal,
            "Account Reactivated", "Your account has been reactivated. You may now log in.",
            user.Id, "User", ct);

        return _mapper.Map<AccountResponseDto>(user);
    }

    // ── Listing ───────────────────────────────────────────────────────────────

    public async Task<PagedResultDto<DoctorAccountResponseDto>> GetAllDoctorsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Doctors.GetPagedAsync(page, pageSize, ct: ct);
        return new PagedResultDto<DoctorAccountResponseDto>
        {
            Items      = _mapper.Map<List<DoctorAccountResponseDto>>(paged.Items),
            TotalCount = paged.TotalCount,
            Page       = paged.Page,
            PageSize   = paged.PageSize
        };
    }

    public async Task<PagedResultDto<AccountResponseDto>> GetAllAdminsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Admins.GetPagedAsync(page, pageSize, ct: ct);
        return new PagedResultDto<AccountResponseDto>
        {
            Items      = _mapper.Map<List<AccountResponseDto>>(paged.Items),
            TotalCount = paged.TotalCount,
            Page       = paged.Page,
            PageSize   = paged.PageSize
        };
    }

    // ── Doctor Profile Management ─────────────────────────────────────────────

    public async Task<DoctorAccountResponseDto> UpdateDoctorProfileAsync(
        int doctorId, UpdateDoctorProfileRequestDto dto, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(doctorId, ct)
            ?? throw new NotFoundException($"Doctor {doctorId} not found.");

        doctor.FullName       = dto.FullName;
        doctor.Phone          = dto.Phone;
        doctor.Specialization = dto.Specialization;
        doctor.Qualification  = dto.Qualification;
        doctor.Bio            = dto.Bio;
        doctor.ExperienceYears = dto.ExperienceYears;

        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<DoctorAccountResponseDto>(doctor);
    }

    public async Task<DoctorAccountResponseDto> UpdateDoctorPhotoAsync(
        int doctorId, UpdateDoctorPhotoRequestDto dto, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(doctorId, ct)
            ?? throw new NotFoundException($"Doctor {doctorId} not found.");

        doctor.PhotoUrl = dto.PhotoUrl;
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<DoctorAccountResponseDto>(doctor);
    }

    // ── Offer Management (BR-58) ──────────────────────────────────────────────

    public async Task<PagedResultDto<OfferStatusResponseDto>> GetAllOffersAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.OfferDiscounts.GetPagedAsync(page, pageSize, ct: ct);
        return new PagedResultDto<OfferStatusResponseDto>
        {
            Items      = _mapper.Map<List<OfferStatusResponseDto>>(paged.Items),
            TotalCount = paged.TotalCount,
            Page       = paged.Page,
            PageSize   = paged.PageSize
        };
    }

    public async Task<OfferStatusResponseDto> ActivateOfferAsync(
        int offerId, CancellationToken ct = default)
    {
        var offer = await _uow.OfferDiscounts.GetByIdAsync(offerId, ct)
            ?? throw new NotFoundException($"Offer {offerId} not found.");

        if (offer.IsActive)
            throw new BusinessRuleException("Offer is already active.");

        if (offer.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new BusinessRuleException("Cannot activate an expired offer.");

        offer.IsActive = true;
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendToRoleAsync(
            UserRole.Patient, NotificationType.OfferActivated, NotificationPriority.Normal,
            "New Offer Available", $"A new offer '{offer.Title}' is now active!",
            offer.Id, "OfferDiscount", ct);

        return _mapper.Map<OfferStatusResponseDto>(offer);
    }

    public async Task<OfferStatusResponseDto> DeactivateOfferAsync(
        int offerId, CancellationToken ct = default)
    {
        var offer = await _uow.OfferDiscounts.GetByIdAsync(offerId, ct)
            ?? throw new NotFoundException($"Offer {offerId} not found.");

        if (!offer.IsActive)
            throw new BusinessRuleException("Offer is already inactive.");

        offer.IsActive = false;
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<OfferStatusResponseDto>(offer);
    }

    // ── Schedule Change Request Approvals (BR-6) ──────────────────────────────

    /// <summary>
    /// Returns all PENDING schedule change requests for the Owner to review.
    /// Return type is PagedResultDto (Application layer) not PagedResult (Domain layer).
    /// Previous build error CS0738 was caused by using the wrong type.
    /// </summary>
    public async Task<PagedResultDto<ScheduleChangeRequestResponseDto>> GetPendingScheduleRequestsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.ScheduleChangeRequests.GetPagedAsync(
            page, pageSize, r => r.Status == RequestStatus.Pending, ct);

        return new PagedResultDto<ScheduleChangeRequestResponseDto>
        {
            Items      = _mapper.Map<List<ScheduleChangeRequestResponseDto>>(paged.Items),
            TotalCount = paged.TotalCount,
            Page       = paged.Page,
            PageSize   = paged.PageSize
        };
    }

    public async Task<ScheduleChangeRequestResponseDto> ApproveScheduleRequestAsync(
        int requestId, int ownerId, CancellationToken ct = default)
    {
        var request = await _uow.ScheduleChangeRequests.GetByIdAsync(requestId, ct)
            ?? throw new NotFoundException($"Schedule change request {requestId} not found.");

        if (request.OwnerId != ownerId)
            throw new ForbiddenException("Only the designated Owner can approve this request.");
        if (request.Status != RequestStatus.Pending)
            throw new BusinessRuleException("Request is no longer pending.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            request.OwnerApproved    = true;
            request.OwnerApprovedAt  = DateTime.UtcNow;
            request.Status           = RequestStatus.Approved;

            // Apply changes to the live schedule
            var schedule = await _uow.Schedules.GetByIdAsync(request.ScheduleId, ct);
            if (schedule != null)
            {
                if (request.ProposedDayOfWeek.HasValue)
                    schedule.DayOfWeek = request.ProposedDayOfWeek.Value;
                if (request.ProposedStartTime.HasValue)
                    schedule.StartTime = request.ProposedStartTime.Value;
                if (request.ProposedEndTime.HasValue)
                    schedule.EndTime = request.ProposedEndTime.Value;
                if (request.ProposedSessionDurationMinutes.HasValue)
                    schedule.SessionDurationMinutes = request.ProposedSessionDurationMinutes.Value;
            }

            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }

        await _notificationService.SendAsync(
            request.RequestingDoctorId, NotificationType.ScheduleChangeRequestApproved,
            NotificationPriority.Normal,
            "Schedule Change Approved",
            "Your schedule change request has been approved and applied.",
            request.Id, "ScheduleChangeRequest", ct);

        return _mapper.Map<ScheduleChangeRequestResponseDto>(request);
    }

    public async Task<ScheduleChangeRequestResponseDto> RejectScheduleRequestAsync(
        int requestId, int ownerId, string? reason, CancellationToken ct = default)
    {
        var request = await _uow.ScheduleChangeRequests.GetByIdAsync(requestId, ct)
            ?? throw new NotFoundException($"Schedule change request {requestId} not found.");

        if (request.OwnerId != ownerId)
            throw new ForbiddenException("Only the designated Owner can reject this request.");
        if (request.Status != RequestStatus.Pending)
            throw new BusinessRuleException("Request is no longer pending.");

        request.Status          = RequestStatus.Rejected;
        request.RejectionReason = reason;   // FIX: was incorrectly setting request.Reason

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            request.RequestingDoctorId, NotificationType.ScheduleChangeRequestRejected,
            NotificationPriority.High,
            "Schedule Change Rejected",
            reason != null
                ? $"Your schedule change request was rejected. Reason: {reason}"
                : "Your schedule change request was rejected by the Owner.",
            request.Id, "ScheduleChangeRequest", ct);

        return _mapper.Map<ScheduleChangeRequestResponseDto>(request);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<User?> GetUserByIdAsync(int userId, CancellationToken ct)
    {
        User? user = await _uow.Patients.GetByIdAsync(userId, ct);
        if (user != null) return user;
        user = await _uow.Doctors.GetByIdAsync(userId, ct);
        if (user != null) return user;
        return await _uow.Admins.GetByIdAsync(userId, ct);
    }

}
