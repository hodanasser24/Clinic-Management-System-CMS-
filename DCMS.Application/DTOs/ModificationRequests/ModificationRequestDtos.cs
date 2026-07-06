using DCMS.Domain.Enums;

namespace DCMS.Application.DTOs.ModificationRequests;

public class CreateServiceModificationRequestDto
{
    public int AdminId { get; set; }
    public int OwnerId { get; set; }
    public int? ServiceId { get; set; }
    public RequestType RequestType { get; set; }
    public string? ProposedName { get; set; }
    public decimal? ProposedPrice { get; set; }
    public string? ProposedDescription { get; set; }
    public int? ProposedEstimatedDurationMinutes { get; set; }
}

public class CreateFAQModificationRequestDto
{
    public int AdminId { get; set; }
    public int OwnerId { get; set; }
    public int? FAQId { get; set; }
    public RequestType RequestType { get; set; }
    public string? ProposedQuestion { get; set; }
    public string? ProposedAnswer { get; set; }
}

public class CreateOfferDiscountModificationRequestDto
{
    public int AdminId { get; set; }
    public int OwnerId { get; set; }
    public int? OfferId { get; set; }
    public RequestType RequestType { get; set; }
    public string? ProposedTitle { get; set; }
    public string? ProposedDescription { get; set; }
    public decimal? ProposedDiscountPercentage { get; set; }
    public DateOnly? ProposedStartDate { get; set; }
    public DateOnly? ProposedEndDate { get; set; }
    public int? ProposedBranchId { get; set; }
    public int? ProposedServiceId { get; set; }
}

public class CreateBranchModificationRequestDto
{
    public int AdminId { get; set; }
    public int OwnerId { get; set; }
    public int? BranchId { get; set; }
    public RequestType RequestType { get; set; }
    public string? ProposedName { get; set; }
    public string? ProposedLocation { get; set; }
    public string? ProposedPhone { get; set; }
    public string? ProposedWorkingHours { get; set; }
}

public class ApproveRejectModificationRequestDto
{
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
}

public class ServiceModificationRequestResponseDto
{
    public int Id { get; set; }
    public int AdminId { get; set; }
    public string AdminName { get; set; } = null!;
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = null!;
    public int? ServiceId { get; set; }
    public RequestType RequestType { get; set; }
    public RequestStatus Status { get; set; }
    public string? ProposedName { get; set; }
    public decimal? ProposedPrice { get; set; }
    public string? ProposedDescription { get; set; }
    public int? ProposedEstimatedDurationMinutes { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class FAQModificationRequestResponseDto
{
    public int Id { get; set; }
    public int AdminId { get; set; }
    public string AdminName { get; set; } = null!;
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = null!;
    public int? FAQId { get; set; }
    public RequestType RequestType { get; set; }
    public RequestStatus Status { get; set; }
    public string? ProposedQuestion { get; set; }
    public string? ProposedAnswer { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OfferDiscountModificationRequestResponseDto
{
    public int Id { get; set; }
    public int AdminId { get; set; }
    public string AdminName { get; set; } = null!;
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = null!;
    public int? OfferId { get; set; }
    public RequestType RequestType { get; set; }
    public RequestStatus Status { get; set; }
    public string? ProposedTitle { get; set; }
    public decimal? ProposedDiscountPercentage { get; set; }
    public DateOnly? ProposedStartDate { get; set; }
    public DateOnly? ProposedEndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class BranchModificationRequestResponseDto
{
    public int Id { get; set; }
    public int AdminId { get; set; }
    public string AdminName { get; set; } = null!;
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = null!;
    public int? BranchId { get; set; }
    public RequestType RequestType { get; set; }
    public RequestStatus Status { get; set; }
    public string? ProposedName { get; set; }
    public string? ProposedLocation { get; set; }
    public string? ProposedPhone { get; set; }
    public string? ProposedWorkingHours { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
