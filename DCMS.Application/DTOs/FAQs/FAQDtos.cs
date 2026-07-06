namespace DCMS.Application.DTOs.FAQs;

public class CreateFAQRequestDto
{
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
    public int DisplayOrder { get; set; }
}

public class UpdateFAQRequestDto
{
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class FAQResponseDto
{
    public int Id { get; set; }
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
