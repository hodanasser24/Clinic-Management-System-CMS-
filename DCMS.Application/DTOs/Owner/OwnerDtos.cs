using DCMS.Domain.Enums;

namespace DCMS.Application.DTOs.Owner;

// ── Account creation DTOs ─────────────────────────────────────────────────────

public class CreateDoctorAccountRequestDto
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Phone { get; set; }
    public string Specialization { get; set; } = null!;
    public string Qualification { get; set; } = null!;
    public string? Bio { get; set; }
    public int ExperienceYears { get; set; }
}

public class CreateAdminAccountRequestDto
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Phone { get; set; }
}

public class AccountResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public bool IsFirstLogin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class DoctorAccountResponseDto : AccountResponseDto
{
    public string Specialization { get; set; } = null!;
    public string Qualification { get; set; } = null!;
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public int ExperienceYears { get; set; }
}

public class DeactivateAccountRequestDto
{
    public string? Reason { get; set; }
}

// ── Offer management DTOs ─────────────────────────────────────────────────────

public class ActivateOfferRequestDto
{
    public int OfferId { get; set; }
}

public class OfferStatusResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string BranchName { get; set; } = null!;
    public decimal DiscountPercentage { get; set; }
}

// ── Profile update DTOs ───────────────────────────────────────────────────────

public class UpdateDoctorProfileRequestDto
{
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string Specialization { get; set; } = null!;
    public string Qualification { get; set; } = null!;
    public string? Bio { get; set; }
    public int ExperienceYears { get; set; }
}

public class UpdateDoctorPhotoRequestDto
{
    public string PhotoUrl { get; set; } = null!;
}
