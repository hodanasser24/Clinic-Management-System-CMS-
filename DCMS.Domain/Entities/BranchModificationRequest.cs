// File: DCMS.Domain/Entities/BranchModificationRequest.cs
using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

public class BranchModificationRequest : BaseEntity
{
    public int AdminId { get; set; }
    public int OwnerId { get; set; }
    public int? BranchId { get; set; }

    public RequestType RequestType { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    // Proposed values for Add/Update
    public string? ProposedName { get; set; }
    public string? ProposedLocation { get; set; }
    public string? ProposedPhone { get; set; }
    public string? ProposedWorkingHours { get; set; }

    public string? Reason { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation Properties
    public virtual Admin Admin { get; set; } = null!;
    public virtual Owner? Owner { get; set; }
    public virtual Branch? Branch { get; set; }
}
