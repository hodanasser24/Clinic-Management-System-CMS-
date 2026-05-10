using DCMS.Application.DTOs.Schedules;
using DCMS.Application.DTOs.Common;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;

namespace DCMS.Application.Services;

public class ScheduleService : IScheduleService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;

    public ScheduleService(IUnitOfWork uow, INotificationService notificationService)
    {
        _uow = uow;
        _notificationService = notificationService;
    }

    public async Task<ScheduleResponseDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var schedule = await _uow.Schedules.GetByIdAsync(id, ct);
        if (schedule == null) throw new NotFoundException($"Schedule {id} not found.");
        return MapToResponse(schedule);
    }

    public async Task<PagedResultDto<ScheduleResponseDto>> GetByDoctorAsync(int doctorId, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Schedules.GetByDoctorAsync(doctorId, page, pageSize, ct);
        return new PagedResultDto<ScheduleResponseDto>
        {
            Items = paged.Items.Select(MapToResponse).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<PagedResultDto<ScheduleResponseDto>> GetByBranchAsync(int branchId, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Schedules.GetByBranchAsync(branchId, page, pageSize, ct);
        return new PagedResultDto<ScheduleResponseDto>
        {
            Items = paged.Items.Select(MapToResponse).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    // BR-30: compute available time slots
    public async Task<List<AvailableSlotDto>> GetAvailableTimeSlotsAsync(int scheduleId, DateOnly date, CancellationToken ct = default)
    {
        var schedule = await _uow.Schedules.GetByIdAsync(scheduleId, ct);
        if (schedule == null || !schedule.IsActive || schedule.DayOfWeek != date.DayOfWeek)
            return new List<AvailableSlotDto>();

        var bookedAppointments = await _uow.Appointments.GetByDoctorAndDateAsync(schedule.DoctorId, date, ct);
        var bookedSlots = bookedAppointments
            .Where(a => a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.Rejected)
            .Select(a => (a.StartTime, a.EndTime))
            .ToList();

        var slots = new List<AvailableSlotDto>();
        var current = schedule.StartTime;

        while (current.AddMinutes(schedule.SessionDurationMinutes) <= schedule.EndTime)
        {
            var slotEnd = current.AddMinutes(schedule.SessionDurationMinutes);
            var isBooked = bookedSlots.Any(b => current < b.EndTime && slotEnd > b.StartTime);

            if (!isBooked)
                slots.Add(new AvailableSlotDto { StartTime = current, EndTime = slotEnd });

            current = slotEnd;
        }

        return slots;
    }

    public async Task<ScheduleResponseDto> CreateAsync(CreateScheduleRequestDto dto, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(dto.DoctorId, ct);
        if (doctor == null) throw new NotFoundException("Doctor not found.");

        var branch = await _uow.Branches.GetByIdAsync(dto.BranchId, ct);
        if (branch == null) throw new NotFoundException("Branch not found.");

        // BR-42: unique per Doctor+Branch+DayOfWeek
        var existing = await _uow.Schedules.GetByDoctorBranchDayAsync(dto.DoctorId, dto.BranchId, dto.DayOfWeek, ct);
        if (existing != null)
            throw new ConflictException("A schedule already exists for this doctor, branch, and day.");

        var schedule = new Schedule
        {
            DoctorId = dto.DoctorId,
            BranchId = dto.BranchId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            SessionDurationMinutes = dto.SessionDurationMinutes,
            IsActive = true
        };

        await _uow.Schedules.AddAsync(schedule, ct);
        await _uow.SaveChangesAsync(ct);
        return MapToResponse(schedule);
    }

    public async Task<ScheduleResponseDto> UpdateAsync(int id, UpdateScheduleRequestDto dto, CancellationToken ct = default)
    {
        var schedule = await _uow.Schedules.GetByIdAsync(id, ct);
        if (schedule == null) throw new NotFoundException($"Schedule {id} not found.");

        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        schedule.SessionDurationMinutes = dto.SessionDurationMinutes;
        schedule.IsActive = dto.IsActive;

        await _uow.SaveChangesAsync(ct);
        return MapToResponse(schedule);
    }

    // SRS §4.2: Admin submits schedule change requests (not Doctor)
    public async Task<ScheduleChangeRequestResponseDto> SubmitChangeRequestAsync(CreateScheduleChangeRequestDto dto, CancellationToken ct = default)
    {
        var admin = await _uow.Admins.GetByIdAsync(dto.RequestingAdminId, ct);
        if (admin == null) throw new NotFoundException("Admin not found.");

        var schedule = await _uow.Schedules.GetByIdAsync(dto.ScheduleId, ct);
        if (schedule == null) throw new NotFoundException("Schedule not found.");

        // Verify the target doctor exists
        var doctor = await _uow.Doctors.GetByIdAsync(schedule.DoctorId, ct);
        if (doctor == null) throw new NotFoundException("Doctor for this schedule not found.");

        var request = new ScheduleChangeRequest
        {
            RequestingDoctorId = schedule.DoctorId,
            AdminId = dto.RequestingAdminId,
            OwnerId = dto.OwnerId,
            ScheduleId = dto.ScheduleId,
            ProposedDayOfWeek = dto.ProposedDayOfWeek,
            ProposedStartTime = dto.ProposedStartTime,
            ProposedEndTime = dto.ProposedEndTime,
            ProposedSessionDurationMinutes = dto.ProposedSessionDurationMinutes,
            Reason = dto.Reason,
            DoctorApproved = false,
            OwnerApproved = false,
            Status = RequestStatus.Pending,
            // BR-7: expires in 24 hours (not 7 days)
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        await _uow.ScheduleChangeRequests.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        // Notify the doctor whose schedule is being changed
        await _notificationService.SendAsync(
            schedule.DoctorId, NotificationType.ScheduleChangeRequestSubmitted, NotificationPriority.Normal,
            "Schedule Change Request", "A schedule change request has been submitted for your schedule and awaits your approval.",
            request.Id, "ScheduleChangeRequest", ct);

        // Notify Owner
        await _notificationService.SendAsync(
            dto.OwnerId, NotificationType.ScheduleChangeRequestSubmitted, NotificationPriority.Normal,
            "Schedule Change Request", "A schedule change request awaits your approval.",
            request.Id, "ScheduleChangeRequest", ct);

        return MapToChangeRequestResponse(request);
    }

    public async Task<ScheduleChangeRequestResponseDto> ApproveChangeRequestByDoctorAsync(int requestId, int doctorId, CancellationToken ct = default)
    {
        var request = await _uow.ScheduleChangeRequests.GetByIdAsync(requestId, ct);
        if (request == null) throw new NotFoundException($"Schedule change request {requestId} not found.");

        if (request.Status != RequestStatus.Pending)
            throw new BusinessRuleException("Request is no longer pending.");

        if (request.ExpiresAt < DateTime.UtcNow)
            throw new BusinessRuleException("This change request has expired.");

        if (request.RequestingDoctorId != doctorId)
            throw new ForbiddenException("Doctor can only approve change requests for their own schedule.");

        request.DoctorApproved = true;
        request.DoctorApprovedAt = DateTime.UtcNow;
        await TryFinalizeAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        return MapToChangeRequestResponse(request);
    }

    public async Task<ScheduleChangeRequestResponseDto> ApproveChangeRequestByOwnerAsync(int requestId, int ownerId, CancellationToken ct = default)
    {
        var request = await _uow.ScheduleChangeRequests.GetByIdAsync(requestId, ct);
        if (request == null) throw new NotFoundException($"Schedule change request {requestId} not found.");

        if (request.Status != RequestStatus.Pending)
            throw new BusinessRuleException("Request is no longer pending.");

        if (request.ExpiresAt < DateTime.UtcNow)
            throw new BusinessRuleException("This change request has expired.");

        request.OwnerApproved = true;
        request.OwnerApprovedAt = DateTime.UtcNow;
        await TryFinalizeAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        return MapToChangeRequestResponse(request);
    }

    // BR-49: Either Doctor OR Owner can reject
    public async Task<ScheduleChangeRequestResponseDto> RejectChangeRequestAsync(int requestId, int rejectingUserId, CancellationToken ct = default)
    {
        var request = await _uow.ScheduleChangeRequests.GetByIdAsync(requestId, ct);
        if (request == null) throw new NotFoundException($"Schedule change request {requestId} not found.");

        if (request.Status != RequestStatus.Pending)
            throw new BusinessRuleException("Request is no longer pending.");

        request.Status = RequestStatus.Rejected;
        await _uow.SaveChangesAsync(ct);

        // Notify the doctor and admin about rejection
        await _notificationService.SendAsync(
            request.RequestingDoctorId, NotificationType.ScheduleChangeRequestRejected, NotificationPriority.High,
            "Schedule Change Rejected", "A schedule change request has been rejected.",
            request.Id, "ScheduleChangeRequest", ct);

        await _notificationService.SendToRoleAsync(
            UserRole.Admin, NotificationType.ScheduleChangeRequestRejected, NotificationPriority.Normal,
            "Schedule Change Rejected", "A schedule change request was rejected.", ct);

        return MapToChangeRequestResponse(request);
    }

    // BR-6: both DoctorApproved AND OwnerApproved required
    private async Task TryFinalizeAsync(ScheduleChangeRequest request, CancellationToken ct)
    {
        if (!request.DoctorApproved || !request.OwnerApproved) return;

        request.Status = RequestStatus.Approved;
        var schedule = await _uow.Schedules.GetByIdAsync(request.ScheduleId, ct);
        if (schedule == null) return;

        if (request.ProposedDayOfWeek.HasValue) schedule.DayOfWeek = request.ProposedDayOfWeek.Value;
        if (request.ProposedStartTime.HasValue) schedule.StartTime = request.ProposedStartTime.Value;
        if (request.ProposedEndTime.HasValue) schedule.EndTime = request.ProposedEndTime.Value;
        if (request.ProposedSessionDurationMinutes.HasValue) schedule.SessionDurationMinutes = request.ProposedSessionDurationMinutes.Value;

        await _notificationService.SendAsync(
            request.RequestingDoctorId, NotificationType.ScheduleChangeRequestApproved, NotificationPriority.Normal,
            "Schedule Change Approved", "Your schedule change request has been approved and applied.",
            request.Id, "ScheduleChangeRequest", ct);

        await _notificationService.SendToRoleAsync(
            UserRole.Admin, NotificationType.ScheduleChangeRequestApproved, NotificationPriority.Normal,
            "Schedule Change Approved", "A schedule change request was approved and applied.", ct);
    }

    private static ScheduleResponseDto MapToResponse(Schedule s) => new()
    {
        Id = s.Id,
        DoctorId = s.DoctorId,
        DoctorName = s.Doctor?.FullName ?? string.Empty,
        BranchId = s.BranchId,
        BranchName = s.Branch?.Name ?? string.Empty,
        DayOfWeek = s.DayOfWeek,
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        SessionDurationMinutes = s.SessionDurationMinutes,
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };

    private static ScheduleChangeRequestResponseDto MapToChangeRequestResponse(ScheduleChangeRequest r) => new()
    {
        Id = r.Id,
        RequestingDoctorId = r.RequestingDoctorId,
        RequestingDoctorName = r.RequestingDoctor?.FullName ?? string.Empty,
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
}
