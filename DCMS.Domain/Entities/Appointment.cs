using System;
using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

public class Appointment : BaseEntity
{
    // Foreign Keys (Required)
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int BranchId { get; set; }
    public int ServiceId { get; set; }

    // DateTime
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    // Statuses
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public AttendanceStatus AttendanceStatus { get; set; } = AttendanceStatus.NotMarked;

    // Urgency
    public bool IsUrgent { get; set; } = false;
    public int? UrgentMarkedBy { get; set; } // FK to Doctor
    public DateTime? UrgentMarkedDate { get; set; }

    // Follow-up
    public bool FollowUpFlag { get; set; } = false;
    public int? PreviousAppointmentId { get; set; } // Self-referencing FK
    
    // Notes
    public string? Notes { get; set; }

    // Auditing
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public int? ConfirmedBy { get; set; } // FK to Admin

    // Navigation Properties
    public virtual Patient Patient { get; set; } = null!;
    public virtual Doctor Doctor { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
    
    public virtual Appointment? PreviousAppointment { get; set; }
    public virtual Report? Report { get; set; }

    public virtual Doctor? UrgentMarkedByDoctor { get; set; }
    public virtual Admin? ConfirmedByAdmin { get; set; }
}
