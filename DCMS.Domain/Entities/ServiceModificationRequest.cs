using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

public class ServiceModificationRequest : BaseEntity
{
    public int AdminId { get; set; }
    public int OwnerId { get; set; }
    public int? ServiceId { get; set; }

    public RequestType RequestType { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public string? ProposedName { get; set; }
    public string? ProposedDescription { get; set; }
    public decimal? ProposedPrice { get; set; }
    public int? ProposedEstimatedDurationMinutes { get; set; }  // Class Spec: was missing

    public string? Reason { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ApprovalDate { get; set; }  // Class Spec: was missing

    public virtual Admin Admin { get; set; } = null!;
    public virtual Owner? Owner { get; set; }
    public virtual Service? Service { get; set; }
}
