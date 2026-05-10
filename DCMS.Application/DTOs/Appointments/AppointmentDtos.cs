using DCMS.Domain.Enums;

namespace DCMS.Application.DTOs.Appointments;

public class AppointmentRequestDto
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int BranchId { get; set; }
    public int ServiceId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public string? Notes { get; set; }
    public int? PreviousAppointmentId { get; set; }
}

public class RescheduleAppointmentRequestDto
{
    public DateOnly NewDate { get; set; }
    public TimeOnly NewStartTime { get; set; }
}

public class ConfirmAppointmentRequestDto
{
    public int AdminId { get; set; }
}

public class RejectAppointmentRequestDto
{
    public int AdminId { get; set; }
}

public class CancelAppointmentRequestDto
{
    public string? Reason { get; set; }
}

public class MarkUrgentRequestDto
{
    public int DoctorId { get; set; }
}

public class MarkAttendanceRequestDto
{
    public AttendanceStatus AttendanceStatus { get; set; }
}

public class AppointmentResponseDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = null!;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = null!;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public AttendanceStatus AttendanceStatus { get; set; }
    public bool IsUrgent { get; set; }
    public bool FollowUpFlag { get; set; }
    public int? UrgentMarkedBy { get; set; }
    public int? ConfirmedBy { get; set; }
    public int? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public int? PreviousAppointmentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AppointmentSummaryDto
{
    public int Id { get; set; }
    public string PatientName { get; set; } = null!;
    public string DoctorName { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public bool IsUrgent { get; set; }
}
