namespace DCMS.Application.DTOs.Branches;

public class CreateBranchRequestDto
{
    public string Name { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? WorkingHours { get; set; }
}

public class UpdateBranchRequestDto
{
    public string Name { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? WorkingHours { get; set; }
    public bool IsActive { get; set; }
}

public class BranchResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? WorkingHours { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
