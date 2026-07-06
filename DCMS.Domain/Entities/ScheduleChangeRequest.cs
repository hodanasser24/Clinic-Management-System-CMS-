using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

/// <summary>
/// Doctor-initiated / Admin-submitted request to change a Schedule.
/// BR-6:  Both Doctor AND Owner must approve.
/// BR-49: Either party can unilaterally reject.
/// BR-7:  Auto-expires after 24 hours if still Pending.
///
/// OldStartTime / OldEndTime added from Class Specification to preserve
/// an audit trail of what was being changed from.
/// </summary>
public class ScheduleChangeRequest : BaseEntity
{
    public int ScheduleId        { get; set; }
    public int AdminId           { get; set; }
    public int RequestingDoctorId { get; set; }
    public int OwnerId           { get; set; }

    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    // ── Audit trail: what the schedule looked like before ─────────────────
    public DayOfWeek? OldDayOfWeek            { get; set; }
    public TimeOnly?  OldStartTime            { get; set; }  // Class Spec
    public TimeOnly?  OldEndTime              { get; set; }  // Class Spec
    public int?       OldSessionDurationMinutes { get; set; }

    // ── Proposed new values ────────────────────────────────────────────────
    public DayOfWeek? ProposedDayOfWeek            { get; set; }
    public TimeOnly?  ProposedStartTime            { get; set; }
    public TimeOnly?  ProposedEndTime              { get; set; }
    public int?       ProposedSessionDurationMinutes { get; set; }
    public int?       ProposedBreakDurationMinutes  { get; set; }

    // ── Dual approval (BR-6) ───────────────────────────────────────────────
    public bool      DoctorApproved   { get; set; } = false;
    public bool      OwnerApproved    { get; set; } = false;
    public DateTime? DoctorApprovedAt { get; set; }
    public DateTime? OwnerApprovedAt  { get; set; }

    public string? Reason          { get; set; }
    public string? RejectionReason { get; set; }

    // BR-7: auto-expire timestamp
    public DateTime ExpiresAt { get; set; }

    // ── Navigation ─────────────────────────────────────────────────────────
    public Schedule   Schedule         { get; set; } = null!;
    public Admin      Admin            { get; set; } = null!;
    public Doctor     RequestingDoctor { get; set; } = null!;
    public Owner      Owner            { get; set; } = null!;
}
