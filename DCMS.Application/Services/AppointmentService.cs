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
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;

    public AppointmentService(IUnitOfWork uow, INotificationService notificationService)
    {
        _uow = uow;
        _notificationService = notificationService;
    }

    public async Task<AppointmentResponseDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(id, ct);
        if (appointment == null) throw new NotFoundException($"Appointment {id} not found.");
        return MapToResponse(appointment);
    }

    public async Task<PagedResultDto<AppointmentSummaryDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Appointments.GetPagedAsync(page, pageSize, ct: ct);
        return new PagedResultDto<AppointmentSummaryDto>
        {
            Items = paged.Items.Select(MapToSummary).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<PagedResultDto<AppointmentSummaryDto>> GetByPatientAsync(int patientId, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Appointments.GetByPatientAsync(patientId, page, pageSize, ct);
        return new PagedResultDto<AppointmentSummaryDto>
        {
            Items = paged.Items.Select(MapToSummary).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<PagedResultDto<AppointmentSummaryDto>> GetByDoctorAsync(int doctorId, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Appointments.GetByDoctorAsync(doctorId, page, pageSize, ct);
        return new PagedResultDto<AppointmentSummaryDto>
        {
            Items = paged.Items.Select(MapToSummary).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<PagedResultDto<AppointmentSummaryDto>> GetUrgentAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Appointments.GetPagedAsync(page, pageSize, a => a.IsUrgent, ct);
        return new PagedResultDto<AppointmentSummaryDto>
        {
            Items = paged.Items.Select(MapToSummary).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<PagedResultDto<AppointmentSummaryDto>> GetUrgentByDateRangeAsync(DateOnly from, DateOnly to, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Appointments.GetPagedAsync(page, pageSize, a => a.IsUrgent && a.Date >= from && a.Date <= to, ct);
        return new PagedResultDto<AppointmentSummaryDto>
        {
            Items = paged.Items.Select(MapToSummary).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<AppointmentResponseDto> BookAsync(AppointmentRequestDto dto, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.GetByIdAsync(dto.PatientId, ct);
        if (patient == null) throw new NotFoundException("Patient not found.");

        var doctor = await _uow.Doctors.GetDoctorWithScheduleAsync(dto.DoctorId, dto.Date.DayOfWeek, ct);
        if (doctor == null) throw new NotFoundException("Doctor not found.");

        var schedule = await _uow.Schedules.GetByDoctorBranchDayAsync(dto.DoctorId, dto.BranchId, dto.Date.DayOfWeek, ct);
        if (schedule == null || !schedule.IsActive)
            throw new BusinessRuleException("No active schedule found for the selected doctor, branch, and day.");

        var endTime = dto.StartTime.AddMinutes(schedule.SessionDurationMinutes);

        // BR-2: guard against double-booking
        var conflict = await _uow.Appointments.HasConflictAsync(dto.DoctorId, dto.Date, dto.StartTime, null, ct);
        if (conflict) throw new ConflictException("Doctor already has an appointment at this time.");

        var appointment = new Appointment
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            BranchId = dto.BranchId,
            ServiceId = dto.ServiceId,
            Date = dto.Date,
            StartTime = dto.StartTime,
            EndTime = endTime,
            Status = AppointmentStatus.Pending,
            AttendanceStatus = AttendanceStatus.NotMarked,
            IsUrgent = false,
            Notes = dto.Notes,
            PreviousAppointmentId = dto.PreviousAppointmentId,
            FollowUpFlag = dto.PreviousAppointmentId.HasValue
        };

        await _uow.Appointments.AddAsync(appointment, ct);
        await _uow.SaveChangesAsync(ct);

        // Notify admin of new pending appointment
        await _notificationService.SendToRoleAsync(
            UserRole.Admin, NotificationType.AppointmentBooked, NotificationPriority.Normal,
            "New Appointment Request", $"A new appointment request is pending confirmation.",
            appointment.Id, "Appointment", ct);

        await _notificationService.SendAsync(
            dto.PatientId, NotificationType.AppointmentBooked, NotificationPriority.Normal,
            "Appointment Booked", "Your appointment has been submitted and is pending confirmation.",
            appointment.Id, "Appointment", ct);

        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponseDto> RescheduleAsync(int id, int requestingUserId, RescheduleAppointmentRequestDto dto, CancellationToken ct = default)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(id, ct);
        if (appointment == null) throw new NotFoundException($"Appointment {id} not found.");

        if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.Rejected)
            throw new BusinessRuleException("Cannot reschedule an appointment in its current state.");

        var schedule = await _uow.Schedules.GetByDoctorBranchDayAsync(appointment.DoctorId, appointment.BranchId, dto.NewDate.DayOfWeek, ct);
        if (schedule == null || !schedule.IsActive)
            throw new BusinessRuleException("No active schedule for the selected date.");

        var conflict = await _uow.Appointments.HasConflictAsync(appointment.DoctorId, dto.NewDate, dto.NewStartTime, id, ct);
        if (conflict) throw new ConflictException("Doctor already has an appointment at the new time.");

        appointment.Date = dto.NewDate;
        appointment.StartTime = dto.NewStartTime;
        appointment.EndTime = dto.NewStartTime.AddMinutes(schedule.SessionDurationMinutes);
        appointment.Status = AppointmentStatus.Pending; // back to pending after reschedule

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            appointment.PatientId, NotificationType.AppointmentRescheduled, NotificationPriority.High,
            "Appointment Rescheduled", $"Your appointment has been rescheduled to {dto.NewDate:yyyy-MM-dd} at {dto.NewStartTime}.",
            appointment.Id, "Appointment", ct);

        await _notificationService.SendAsync(
            appointment.DoctorId, NotificationType.AppointmentRescheduled, NotificationPriority.Normal,
            "Appointment Rescheduled", "An appointment in your schedule has been rescheduled.",
            appointment.Id, "Appointment", ct);

        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponseDto> ConfirmAsync(int id, ConfirmAppointmentRequestDto dto, CancellationToken ct = default)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(id, ct);
        if (appointment == null) throw new NotFoundException($"Appointment {id} not found.");

        if (appointment.Status != AppointmentStatus.Pending)
            throw new BusinessRuleException("Only pending appointments can be confirmed.");

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.ConfirmedBy = dto.AdminId;
        appointment.ConfirmedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            appointment.PatientId, NotificationType.AppointmentConfirmed, NotificationPriority.High,
            "Appointment Confirmed", "Your appointment has been confirmed.",
            appointment.Id, "Appointment", ct);

        await _notificationService.SendAsync(
            appointment.DoctorId, NotificationType.AppointmentConfirmed, NotificationPriority.Normal,
            "Appointment Confirmed", "An appointment has been confirmed in your schedule.",
            appointment.Id, "Appointment", ct);

        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponseDto> RejectAsync(int id, RejectAppointmentRequestDto dto, CancellationToken ct = default)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(id, ct);
        if (appointment == null) throw new NotFoundException($"Appointment {id} not found.");

        if (appointment.Status != AppointmentStatus.Pending)
            throw new BusinessRuleException("Only pending appointments can be rejected.");

        appointment.Status = AppointmentStatus.Rejected;
        appointment.ConfirmedBy = dto.AdminId;
        appointment.RejectedAt = DateTime.UtcNow;
        appointment.RejectedBy = dto.AdminId;

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            appointment.PatientId, NotificationType.AppointmentRejected, NotificationPriority.High,
            "Appointment Rejected", "Your appointment request has been rejected.",
            appointment.Id, "Appointment", ct);

        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponseDto> CancelAsync(int id, int requestingUserId, CancelAppointmentRequestDto dto, CancellationToken ct = default)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(id, ct);
        if (appointment == null) throw new NotFoundException($"Appointment {id} not found.");

        if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Cancelled)
            throw new BusinessRuleException("Appointment cannot be cancelled in its current state.");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.CancelledBy = requestingUserId;

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            appointment.PatientId, NotificationType.AppointmentCancelled, NotificationPriority.High,
            "Appointment Cancelled", "Your appointment has been cancelled.",
            appointment.Id, "Appointment", ct);

        await _notificationService.SendAsync(
            appointment.DoctorId, NotificationType.AppointmentCancelled, NotificationPriority.Normal,
            "Appointment Cancelled", "An appointment has been cancelled.",
            appointment.Id, "Appointment", ct);

        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponseDto> MarkUrgentAsync(int id, MarkUrgentRequestDto dto, CancellationToken ct = default)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(id, ct);
        if (appointment == null) throw new NotFoundException($"Appointment {id} not found.");

        if (appointment.DoctorId != dto.DoctorId)
            throw new ForbiddenException("Only the appointment's assigned doctor can mark it as urgent.");

        if (appointment.IsUrgent)
            throw new BusinessRuleException("Appointment is already marked as urgent.");

        appointment.IsUrgent = true;
        appointment.UrgentMarkedBy = dto.DoctorId;
        appointment.UrgentMarkedDate = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        // SRS §4.6: urgent notification goes to DOCTOR and ADMIN only — NOT patient
        await _notificationService.SendAsync(
            appointment.DoctorId, NotificationType.AppointmentMarkedUrgent, NotificationPriority.High,
            "Urgent Case", $"Appointment #{appointment.Id} has been flagged as urgent.",
            appointment.Id, "Appointment", ct);

        await _notificationService.SendToRoleAsync(
            UserRole.Admin, NotificationType.AppointmentMarkedUrgent, NotificationPriority.High,
            "Urgent Case Alert", $"Appointment #{appointment.Id} for patient has been flagged as urgent.",
            appointment.Id, "Appointment", ct);

        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponseDto> UnmarkUrgentAsync(int id, int requestingDoctorId, CancellationToken ct = default)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(id, ct);
        if (appointment == null) throw new NotFoundException($"Appointment {id} not found.");

        // BR-56: only the doctor who marked urgent can unmark
        if (appointment.UrgentMarkedBy != requestingDoctorId)
            throw new ForbiddenException("Only the doctor who marked this appointment as urgent can unmark it.");

        appointment.IsUrgent = false;
        appointment.UrgentMarkedBy = null;
        appointment.UrgentMarkedDate = null;

        await _uow.SaveChangesAsync(ct);
        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponseDto> MarkAttendanceAsync(int id, MarkAttendanceRequestDto dto, CancellationToken ct = default)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(id, ct);
        if (appointment == null) throw new NotFoundException($"Appointment {id} not found.");

        if (appointment.Status != AppointmentStatus.Confirmed)
            throw new BusinessRuleException("Attendance can only be marked for confirmed appointments.");

        // BR-37: UTC-safe date comparison
        var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);
        if (appointment.Date > todayUtc)
            throw new BusinessRuleException("Attendance cannot be marked for a future appointment.");

        appointment.AttendanceStatus = dto.AttendanceStatus;

        if (dto.AttendanceStatus == AttendanceStatus.Attended)
        {
            appointment.Status = AppointmentStatus.Completed;
            appointment.CompletedAt = DateTime.UtcNow;
        }

        await _uow.SaveChangesAsync(ct);
        return MapToResponse(appointment);
    }

    private static AppointmentResponseDto MapToResponse(Appointment a) => new()
    {
        Id = a.Id,
        PatientId = a.PatientId,
        PatientName = a.Patient?.FullName ?? string.Empty,
        DoctorId = a.DoctorId,
        DoctorName = a.Doctor?.FullName ?? string.Empty,
        BranchId = a.BranchId,
        BranchName = a.Branch?.Name ?? string.Empty,
        ServiceId = a.ServiceId,
        ServiceName = a.Service?.Name ?? string.Empty,
        Date = a.Date,
        StartTime = a.StartTime,
        EndTime = a.EndTime,
        Status = a.Status,
        AttendanceStatus = a.AttendanceStatus,
        IsUrgent = a.IsUrgent,
        UrgentMarkedBy = a.UrgentMarkedBy,
        ConfirmedBy = a.ConfirmedBy,
        Notes = a.Notes,
        PreviousAppointmentId = a.PreviousAppointmentId,
        FollowUpFlag = a.FollowUpFlag,
        CancelledAt = a.CancelledAt,
        CancelledBy = a.CancelledBy,
        RejectedAt = a.RejectedAt,
        RejectedBy = a.RejectedBy,
        CompletedAt = a.CompletedAt,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };

    private static AppointmentSummaryDto MapToSummary(Appointment a) => new()
    {
        Id = a.Id,
        PatientName = a.Patient?.FullName ?? string.Empty,
        DoctorName = a.Doctor?.FullName ?? string.Empty,
        BranchName = a.Branch?.Name ?? string.Empty,
        ServiceName = a.Service?.Name ?? string.Empty,
        Date = a.Date,
        StartTime = a.StartTime,
        EndTime = a.EndTime,
        Status = a.Status,
        IsUrgent = a.IsUrgent
    };
}
