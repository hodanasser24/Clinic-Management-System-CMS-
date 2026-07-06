// File: DCMS.Domain/Entities/FAQModificationRequest.cs
using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

public class FAQModificationRequest : BaseEntity
{
    public int AdminId { get; set; }
    public int OwnerId { get; set; }
    public int? FAQId { get; set; }

    public RequestType RequestType { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    // Proposed values for Add/Update
    public string? ProposedQuestion { get; set; }
    public string? ProposedAnswer { get; set; }

    public string? Reason { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation Properties
    public virtual Admin Admin { get; set; } = null!;
    public virtual Owner? Owner { get; set; }
    public virtual FAQ? FAQ { get; set; }
}
