using DCMS.Domain.Enums;

namespace DCMS.Application.DTOs.Reports;

public class CreateReportRequestDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string Diagnosis { get; set; } = null!;
    public string? Treatment { get; set; }
    public string? InternalNotes { get; set; }
    public CaseStatus CaseStatus { get; set; }
    // Class Spec clinical fields
    public string? FollowUpInstructions { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? DietInstructions { get; set; }
    public string? AllowedFood { get; set; }
    public string? RestrictedFood { get; set; }
    public string? HomeCareInstructions { get; set; }
}

public class UpdateReportRequestDto
{
    public string Diagnosis { get; set; } = null!;
    public string? Treatment { get; set; }
    public string? InternalNotes { get; set; }
    public CaseStatus CaseStatus { get; set; }
    public string? FollowUpInstructions { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? DietInstructions { get; set; }
    public string? AllowedFood { get; set; }
    public string? RestrictedFood { get; set; }
    public string? HomeCareInstructions { get; set; }
}

public class ReportResponseDto
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = null!;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = null!;
    public string Diagnosis { get; set; } = null!;
    public string? Treatment { get; set; }
    public CaseStatus CaseStatus { get; set; }
    // Patient-visible clinical fields
    public string? FollowUpInstructions { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? DietInstructions { get; set; }
    public string? HomeCareInstructions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class DoctorReportResponseDto : ReportResponseDto
{
    // BR-57: Additional fields visible only to Doctor/Owner/Admin
    public string? InternalNotes { get; set; }
    public string? AllowedFood { get; set; }
    public string? RestrictedFood { get; set; }
}

/// <summary>
/// Report comparison result – used to track a patient's dental health over time.
/// </summary>
public class ReportCompareResponseDto
{
    public ReportResponseDto              EarlierReport          { get; set; } = null!;
    public ReportResponseDto              LaterReport            { get; set; } = null!;
    public IEnumerable<ToothDiffDto>      ToothDiffs             { get; set; } = [];
    public int                            ChangedCount           { get; set; }
    public int                            ImprovedCount          { get; set; }
    public int                            WorsendCount           { get; set; }
    public string                         DiagnosisChangeSummary { get; set; } = string.Empty;
}

public class ToothDiffDto
{
    public int                    ToothNumber  { get; set; }
    public DCMS.Domain.Enums.ToothStatus? BeforeStatus { get; set; }
    public DCMS.Domain.Enums.ToothStatus? AfterStatus  { get; set; }
    public string?                BeforeLabel  { get; set; }
    public string?                AfterLabel   { get; set; }
    public bool                   Changed      { get; set; }
}
