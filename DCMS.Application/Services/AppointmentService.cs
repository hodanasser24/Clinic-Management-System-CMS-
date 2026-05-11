using DCMS.Application.DTOs.Appointments;
using DCMS.Application.DTOs.Common;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;

namespace DCMS.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork          _uow;
    private readonly INotificationService _notificationService;

    public AppointmentService(IUnitOfWork uow, INotificationService notificationService)
    {
        _uow                 = uow;
        _notificationService = notificationService;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<AppointmentResponseDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var a = await _uow.Appointments.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException($"Appointment {id} not found.");
        return MapToResponse(a);
    }

    public async Task<PagedResultDto<AppointmentSummaryDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Appointments.GetPagedWithDetailsAsync(page, pageSize, ct: ct);
        return ToSummaryPaged(paged);
    }

    public async Task<PagedResultDto<AppointmentSummaryDto>> GetByPatientAsync(
        int patientId, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Appointments.GetByPatientWithDetailsAsync(patientId, page, pageSize, ct);
        return ToSummaryPaged(paged);
    }

    public async Task<PagedResultDto<AppointmentSummaryDto>> GetByDoctorAsync(
        int doctorId, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Appointments.GetByDoctorWithDetailsAsync(doctorId, page, pageSize, ct);
        return ToSummaryPaged(paged);
    }

    public async Task<PagedResultDto<AppointmentSummaryDto>> GetUrgentAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Appointments.GetPagedWithDetailsAsync(page, pageSize, a => a.IsUrgent, ct);
        return ToSummaryPaged(paged);
    }

    public async Task<PagedResultDto<AppointmentSummaryDto>> GetUrgentByDateRangeAsync(
        DateOnly from, DateOnly to, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Appointments.GetPagedWithDetailsAsync(
            page, pageSize, a => a.IsUrgent && a.Date >= from && a.Date <= to, ct);
        return ToSummaryPaged(paged);
    }

    public async Task<PagedResultDto<AppointmentSummaryDto>> GetUpcomingByPatientAsync(
        int patientId, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Appointments.GetUpcomingByPatientAsync(patientId, page, pageSize, ct);
        return ToSummaryPaged(paged);
    }

    public async Task<PagedResultDto<AppointmentSummaryDto>> GetHistoryByPatientAsync(
        int patientId, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Appointments.GetHistoryByPatientAsync(patientId, page, pageSize, ct);
        return ToSummaryPaged(paged);
    }

    // ── Mutations ─────────────────────────────────────────────────────────────

    public async Task<AppointmentResponseDto> BookAsync(AppointmentRequestDto dto, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.GetByIdAsync(dto.PatientId, ct)
            ?? throw new NotFoundException("Patient not found.");

        var schedule = await _uow.Schedules.GetByDoctorBranchDayAsync(
            dto.DoctorId, dto.BranchId, dto.Date.DayOfWeek, ct);
        if (schedule == null || !schedule.IsActive)
            throw new BusinessRuleException(
                "No active schedule found for the selected doctor, branch, and day.");

        // BR-2: prevent double-booking
        var conflict = await _uow.Appointments.HasConflictAsync(
            dto.DoctorId, dto.Date, dto.StartTime, null, ct);
        if (conflict)
            throw new ConflictException("Doctor already has an appointment at this time.");

        var endTime = dto.StartTime.AddMinutes(schedule.SessionDurationMinutes);

        var appointment = new Appointment
        {
            PatientId             = dto.PatientId,
            DoctorId              = dto.DoctorId,
            BranchId              = dto.BranchId,
            ServiceId             = dto.ServiceId,
            Date                  = dto.Date,
            StartTime             = dto.StartTime,
            EndTime               = endTime,
            Status                = AppointmentStatus.Pending,
            AttendanceStatus      = AttendanceStatus.NotMarked,
            IsUrgent              = false,
            Notes                 = dto.Notes,
            PreviousAppointmentId = dto.PreviousAppointmentId,
            FollowUpFlag          = dto.PreviousAppointmentId.HasValue,
            RequestedAt           = DateTime.UtcNow
        };

        await _uow.Appointments.AddAsync(appointment, ct);
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendToRoleAsync(
            UserRole.Admin, NotificationType.AppointmentBooked, NotificationPriority.Normal,
            "New Appointment Request", "A new appointment request is pending confirmation.",
            appointment.Id, "Appointment", ct);

        await _notificationService.SendAsync(
            dto.PatientId, NotificationType.AppointmentBooked, NotificationPriority.Normal,
            "Appointment Booked", "Your appointment is submitted and awaiting confirmation.",
            appointment.Id, "Appointment", ct);

        var full = await _uow.Appointments.GetByIdWithDetailsAsync(appointment.Id, ct) ?? appointment;
        return MapToResponse(full);
    }

    public async Task<AppointmentResponseDto> RescheduleAsync(
        int id, int requestingUserId, RescheduleAppointmentRequestDto dto, CancellationToken ct = default)
    {
        var a = await _uow.Appointments.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException($"Appointment {id} not found.");

        if (a.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.Rejected)
            throw new BusinessRuleException("Cannot reschedule an appointment in its current state.");

        var schedule = await _uow.Schedules.GetByDoctorBranchDayAsync(
            a.DoctorId, a.BranchId, dto.NewDate.DayOfWeek, ct);
        if (schedule == null || !schedule.IsActive)
            throw new BusinessRuleException("No active schedule for the selected date.");

        var conflict = await _uow.Appointments.HasConflictAsync(
            a.DoctorId, dto.NewDate, dto.NewStartTime, id, ct);
        if (conflict)
            throw new ConflictException("Doctor already has an appointment at the new time.");

        a.Date      = dto.NewDate;
        a.StartTime = dto.NewStartTime;
        a.EndTime   = dto.NewStartTime.AddMinutes(schedule.SessionDurationMinutes);
        a.Status    = AppointmentStatus.Pending;

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            a.PatientId, NotificationType.AppointmentRescheduled, NotificationPriority.High,
            "Appointment Rescheduled",
            $"Your appointment was rescheduled to {dto.NewDate:yyyy-MM-dd} at {dto.NewStartTime}.",
            a.Id, "Appointment", ct);

        await _notificationService.SendAsync(
            a.DoctorId, NotificationType.AppointmentRescheduled, NotificationPriority.Normal,
            "Appointment Rescheduled", "An appointment in your schedule has been rescheduled.",
            a.Id, "Appointment", ct);

        return MapToResponse(a);
    }

    public async Task<AppointmentResponseDto> ConfirmAsync(
        int id, ConfirmAppointmentRequestDto dto, CancellationToken ct = default)
    {
        var a = await _uow.Appointments.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException($"Appointment {id} not found.");

        if (a.Status != AppointmentStatus.Pending)
            throw new BusinessRuleException("Only pending appointments can be confirmed.");

        a.Status      = AppointmentStatus.Confirmed;
        a.ConfirmedBy = dto.AdminId;
        a.ConfirmedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            a.PatientId, NotificationType.AppointmentConfirmed, NotificationPriority.High,
            "Appointment Confirmed", "Your appointment has been confirmed.",
            a.Id, "Appointment", ct);

        await _notificationService.SendAsync(
            a.DoctorId, NotificationType.AppointmentConfirmed, NotificationPriority.Normal,
            "Appointment Confirmed", "An appointment has been confirmed in your schedule.",
            a.Id, "Appointment", ct);

        return MapToResponse(a);
    }

    public async Task<AppointmentResponseDto> RejectAsync(
        int id, RejectAppointmentRequestDto dto, CancellationToken ct = default)
    {
        var a = await _uow.Appointments.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException($"Appointment {id} not found.");

        if (a.Status != AppointmentStatus.Pending)
            throw new BusinessRuleException("Only pending appointments can be rejected.");

        a.Status     = AppointmentStatus.Rejected;
        a.RejectedAt = DateTime.UtcNow;
        a.RejectedBy = dto.AdminId;

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            a.PatientId, NotificationType.AppointmentRejected, NotificationPriority.High,
            "Appointment Rejected", "Your appointment request has been rejected.",
            a.Id, "Appointment", ct);

        return MapToResponse(a);
    }

    public async Task<AppointmentResponseDto> CancelAsync(
        int id, int requestingUserId, CancelAppointmentRequestDto dto, CancellationToken ct = default)
    {
        var a = await _uow.Appointments.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException($"Appointment {id} not found.");

        if (a.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            throw new BusinessRuleException("Appointment cannot be cancelled in its current state.");

        a.Status      = AppointmentStatus.Cancelled;
        a.CancelledAt = DateTime.UtcNow;
        a.CancelledBy = requestingUserId;

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            a.PatientId, NotificationType.AppointmentCancelled, NotificationPriority.High,
            "Appointment Cancelled", "Your appointment has been cancelled.",
            a.Id, "Appointment", ct);

        await _notificationService.SendAsync(
            a.DoctorId, NotificationType.AppointmentCancelled, NotificationPriority.Normal,
            "Appointment Cancelled", "An appointment has been cancelled from your schedule.",
            a.Id, "Appointment", ct);

        return MapToResponse(a);
    }

    public async Task<AppointmentResponseDto> MarkUrgentAsync(
        int id, MarkUrgentRequestDto dto, CancellationToken ct = default)
    {
        var a = await _uow.Appointments.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException($"Appointment {id} not found.");

        if (a.DoctorId != dto.DoctorId)
            throw new ForbiddenException("Only the appointment's assigned doctor can mark it as urgent.");

        if (a.IsUrgent)
            throw new BusinessRuleException("Appointment is already marked as urgent.");

        a.IsUrgent        = true;
        a.UrgentMarkedBy  = dto.DoctorId;
        a.UrgentMarkedDate = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        // SRS: urgent notifications go to Doctor and Admin only — NEVER Patient
        await _notificationService.SendAsync(
            a.DoctorId, NotificationType.AppointmentMarkedUrgent, NotificationPriority.High,
            "Urgent Case Flagged", $"Appointment #{a.Id} has been flagged as urgent.",
            a.Id, "Appointment", ct);

        await _notificationService.SendToRoleAsync(
            UserRole.Admin, NotificationType.AppointmentMarkedUrgent, NotificationPriority.High,
            "Urgent Case Alert", $"Appointment #{a.Id} for patient {a.Patient?.FullName} has been flagged as urgent.",
            a.Id, "Appointment", ct);

        return MapToResponse(a);
    }

    public async Task<AppointmentResponseDto> UnmarkUrgentAsync(
        int id, int requestingDoctorId, CancellationToken ct = default)
    {
        var a = await _uow.Appointments.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException($"Appointment {id} not found.");

        // BR-56: only the doctor who marked it can unmark
        if (a.UrgentMarkedBy != requestingDoctorId)
            throw new ForbiddenException(
                "Only the doctor who marked this appointment as urgent can unmark it.");

        a.IsUrgent        = false;
        a.UrgentMarkedBy  = null;
        a.UrgentMarkedDate = null;

        await _uow.SaveChangesAsync(ct);
        return MapToResponse(a);
    }

    public async Task<AppointmentResponseDto> MarkAttendanceAsync(
        int id, MarkAttendanceRequestDto dto, CancellationToken ct = default)
    {
        var a = await _uow.Appointments.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException($"Appointment {id} not found.");

        if (a.Status != AppointmentStatus.Confirmed)
            throw new BusinessRuleException("Attendance can only be marked for confirmed appointments.");

        // BR-37: validate that the appointment's date AND end-time have passed (UTC)
        var nowUtc            = DateTime.UtcNow;
        var appointmentEndUtc = a.Date.ToDateTime(a.EndTime, DateTimeKind.Utc);

        if (nowUtc < appointmentEndUtc)
            throw new BusinessRuleException(
                $"Attendance cannot be marked before the appointment ends " +
                $"({a.Date:yyyy-MM-dd} at {a.EndTime}).");

        a.AttendanceStatus = dto.AttendanceStatus;

        if (dto.AttendanceStatus == AttendanceStatus.Attended)
        {
            a.Status      = AppointmentStatus.Completed;
            a.CompletedAt = nowUtc;

            // Track revenue record (Income requirement)
            if (a.Service != null)
            {
                var revenue = new Revenue
                {
                    AppointmentId = a.Id,
                    PatientId     = a.PatientId,
                    BranchId      = a.BranchId,
                    ServiceId     = a.ServiceId,
                    Amount        = a.Service.Price,
                    Date          = a.Date
                };
                await _uow.Revenues.AddAsync(revenue, ct);
            }
        }

        await _uow.SaveChangesAsync(ct);
        return MapToResponse(a);
    }

    // ── Mapping helpers ───────────────────────────────────────────────────────

    private static AppointmentResponseDto MapToResponse(Appointment a) => new()
    {
        Id                    = a.Id,
        PatientId             = a.PatientId,
        PatientName           = a.Patient?.FullName ?? string.Empty,
        DoctorId              = a.DoctorId,
        DoctorName            = a.Doctor?.FullName  ?? string.Empty,
        BranchId              = a.BranchId,
        BranchName            = a.Branch?.Name      ?? string.Empty,
        ServiceId             = a.ServiceId,
        ServiceName           = a.Service?.Name     ?? string.Empty,
        Date                  = a.Date,
        StartTime             = a.StartTime,
        EndTime               = a.EndTime,
        Status                = a.Status,
        AttendanceStatus      = a.AttendanceStatus,
        IsUrgent              = a.IsUrgent,
        UrgentMarkedBy        = a.UrgentMarkedBy,
        ConfirmedBy           = a.ConfirmedBy,
        Notes                 = a.Notes,
        PreviousAppointmentId = a.PreviousAppointmentId,
        FollowUpFlag          = a.FollowUpFlag,
        CancelledAt           = a.CancelledAt,
        CancelledBy           = a.CancelledBy,
        RejectedAt            = a.RejectedAt,
        RejectedBy            = a.RejectedBy,
        CompletedAt           = a.CompletedAt,
        CreatedAt             = a.CreatedAt,
        UpdatedAt             = a.UpdatedAt
    };

    private static AppointmentSummaryDto MapToSummary(Appointment a) => new()
    {
        Id          = a.Id,
        PatientName = a.Patient?.FullName ?? string.Empty,
        DoctorName  = a.Doctor?.FullName  ?? string.Empty,
        BranchName  = a.Branch?.Name      ?? string.Empty,
        ServiceName = a.Service?.Name     ?? string.Empty,
        Date        = a.Date,
        StartTime   = a.StartTime,
        EndTime     = a.EndTime,
        Status      = a.Status,
        IsUrgent    = a.IsUrgent
    };

    private static PagedResultDto<AppointmentSummaryDto> ToSummaryPaged(
        DCMS.Domain.Interfaces.Repositories.PagedResult<Appointment> paged) => new()
    {
        Items      = paged.Items.Select(MapToSummary).ToList(),
        TotalCount = paged.TotalCount,
        Page       = paged.Page,
        PageSize   = paged.PageSize
    };
}
