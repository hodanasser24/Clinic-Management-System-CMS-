using System;
using DCMS.Domain.Common;

namespace DCMS.Domain.Entities;

public class OfferDiscount : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPercentage { get; set; }
    
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    
    public bool IsActive { get; set; }
    
    public int BranchId { get; set; }
    public int? ServiceId { get; set; }

    public virtual Branch Branch { get; set; } = null!;
    public virtual Service? Service { get; set; }
}
