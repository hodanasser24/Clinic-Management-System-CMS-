// File: DCMS.Domain/Entities/OfferDiscountModificationRequest.cs
using System;
using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

public class OfferDiscountModificationRequest : BaseEntity
{
    public int AdminId { get; set; }
    public int OwnerId { get; set; }
    public int? OfferId { get; set; }

    public RequestType RequestType { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    // Proposed values for Add/Update
    public string? ProposedTitle { get; set; }
    public string? ProposedDescription { get; set; }
    public decimal? ProposedDiscountPercentage { get; set; }
    public DateOnly? ProposedStartDate { get; set; }
    public DateOnly? ProposedEndDate { get; set; }
    public int? ProposedBranchId { get; set; }
    public int? ProposedServiceId { get; set; }

    public string? Reason { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation Properties
    public virtual Admin Admin { get; set; } = null!;
    public virtual Owner? Owner { get; set; }
    public virtual OfferDiscount? OfferDiscount { get; set; }
}
