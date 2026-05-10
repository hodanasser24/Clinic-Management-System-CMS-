using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

/// <summary>
/// Represents a Doctor-initiated request to change their own schedule.
/// Requires dual approval: both the requesting Doctor AND the Owner must approve (BR-6).
/// Either party can unilaterally reject (BR-49).
/// Auto-expires after 24 hours if still Pending (BR-7).
///
/// IMPROVE-3: The FK to the requesting Doctor is named RequestingDoctorId (not DoctorId)
///            to disambiguate it from Schedule.DoctorId (the schedule-owning Doctor).
///            This prevents developer errors when implementing the BR-6 approval chain.
/// </summary>
public class ScheduleChangeRequest : BaseEntity
{
    public int ScheduleId { get; set; }

    /// <summary>
    /// The Admin who submitted this change request on behalf of (or alongside) the Doctor.
    /// </summary>
    public int AdminId { get; set; }

    /// <summary>
    /// IMPROVE-3: The Doctor who initiated this schedule change request.
    /// Note: the schedule-owning Doctor is reachable via Schedule.DoctorId.
    /// </summary>
    public int RequestingDoctorId { get; set; }

    public int OwnerId { get; set; }

    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    // ── Proposed changes (nullable = keep existing value) ──────────────────
    public DayOfWeek? ProposedDayOfWeek { get; set; }
    public TimeOnly? ProposedStartTime { get; set; }
    public TimeOnly? ProposedEndTime { get; set; }
    public int? ProposedSessionDurationMinutes { get; set; }
    public int? ProposedBreakDurationMinutes { get; set; }

    // ── Dual approval fields (BR-6) ────────────────────────────────────────
    /// <summary>Set to true when the requesting Doctor confirms the change.</summary>
    public bool DoctorApproved { get; set; } = false;

    /// <summary>Set to true when the Owner approves the change.</summary>
    public bool OwnerApproved { get; set; } = false;

    public DateTime? DoctorApprovedAt { get; set; }
    public DateTime? OwnerApprovedAt { get; set; }

    public string? Reason { get; set; }
    public string? RejectionReason { get; set; }

    // BR-7: Auto-expire after 24h
    public DateTime ExpiresAt { get; set; }

    // ── Navigation properties ──────────────────────────────────────────────
    public Schedule Schedule { get; set; } = null!;
    public Admin Admin { get; set; } = null!;

    /// <summary>IMPROVE-3: Navigation renamed to match RequestingDoctorId.</summary>
    public Doctor RequestingDoctor { get; set; } = null!;

    public Owner Owner { get; set; } = null!;
}
