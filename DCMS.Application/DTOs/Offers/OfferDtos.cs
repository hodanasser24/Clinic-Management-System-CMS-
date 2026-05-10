namespace DCMS.Application.DTOs.Offers;

public class CreateOfferDiscountRequestDto
{
    public string Title { get; set; } = null!;
    public decimal DiscountPercentage { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int BranchId { get; set; }
    public int? ServiceId { get; set; }
}

public class UpdateOfferDiscountRequestDto
{
    public string Title { get; set; } = null!;
    public decimal DiscountPercentage { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }
    public int? ServiceId { get; set; }
}

public class OfferDiscountResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public decimal DiscountPercentage { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
