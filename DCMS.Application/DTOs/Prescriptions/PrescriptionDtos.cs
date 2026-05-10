using DCMS.Domain.Enums;

namespace DCMS.Application.DTOs.Prescriptions;

public class CreatePrescriptionRequestDto
{
    public int ReportId { get; set; }
    public string? GeneralInstructions { get; set; }
    public List<CreatePrescriptionItemRequestDto> Items { get; set; } = new();
}

public class CreatePrescriptionItemRequestDto
{
    public string MedicationName { get; set; } = null!;
    public string Dosage { get; set; } = null!;
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public MedicationRoute Route { get; set; }
    public string? Notes { get; set; }
}

public class PrescriptionResponseDto
{
    public int Id { get; set; }
    public int ReportId { get; set; }
    public string? GeneralInstructions { get; set; }
    public List<PrescriptionItemResponseDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PrescriptionItemResponseDto
{
    public int Id { get; set; }
    public int PrescriptionId { get; set; }
    public string MedicationName { get; set; } = null!;
    public string Dosage { get; set; } = null!;
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public MedicationRoute Route { get; set; }
    public string? Notes { get; set; }
}
