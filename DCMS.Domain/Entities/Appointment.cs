using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

public class Appointment : BaseEntity
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int BranchId { get; set; }
    public int ServiceId { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public AttendanceStatus AttendanceStatus { get; set; } = AttendanceStatus.NotMarked;

    public bool IsUrgent { get; set; } = false;
    public int? UrgentMarkedBy { get; set; }
    public DateTime? UrgentMarkedDate { get; set; }

    public bool FollowUpFlag { get; set; } = false;
    public int? PreviousAppointmentId { get; set; }

    public string? Notes { get; set; }

    public DateTime RequestedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public int? ConfirmedBy { get; set; }

    // Cancellation tracking
    public DateTime? CancelledAt { get; set; }
    public int? CancelledBy { get; set; }

    // Rejection tracking
    public DateTime? RejectedAt { get; set; }
    public int? RejectedBy { get; set; }

    // Completion tracking
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public Appointment? PreviousAppointment { get; set; }
    public Doctor? UrgentMarkedByDoctor { get; set; }
    public Admin? ConfirmedByAdmin { get; set; }
    public Report? Report { get; set; }
}
