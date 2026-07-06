using DCMS.Domain.Enums;

namespace DCMS.Application.DTOs.Reports;

public class CreateReportRequestDto
{
    public int    AppointmentId { get; set; }
    public int    PatientId    { get; set; }
    public int    DoctorId     { get; set; }
    public string Diagnosis    { get; set; } = null!;
    public string? Treatment   { get; set; }
    public string? Notes       { get; set; }          // Public notes (all roles)
    public string? InternalNotes { get; set; }         // Doctor/Owner only (BR-57)
    public CaseStatus CaseStatus { get; set; }
    // Extended clinical fields
    public string? FollowUpInstructions { get; set; }
    public string? TreatmentPlan        { get; set; }
    public string? DietInstructions     { get; set; }
    public string? AllowedFood          { get; set; }
    public string? RestrictedFood       { get; set; }
    public string? HomeCareInstructions { get; set; }
}

public class UpdateReportRequestDto
{
    public string  Diagnosis     { get; set; } = null!;
    public string? Treatment     { get; set; }
    public string? Notes         { get; set; }
    public string? InternalNotes { get; set; }
    public CaseStatus CaseStatus { get; set; }
    public string? FollowUpInstructions { get; set; }
    public string? TreatmentPlan        { get; set; }
    public string? DietInstructions     { get; set; }
    public string? AllowedFood          { get; set; }
    public string? RestrictedFood       { get; set; }
    public string? HomeCareInstructions { get; set; }
}

/// <summary>Base response — visible to ALL authorised roles including Patient.</summary>
public class ReportResponseDto
{
    public int    Id            { get; set; }
    public int    AppointmentId { get; set; }
    public int    PatientId     { get; set; }
    public string PatientName   { get; set; } = null!;
    public int    DoctorId      { get; set; }
    public string DoctorName    { get; set; } = null!;
    public string Diagnosis     { get; set; } = null!;
    public string? Treatment    { get; set; }
    public string? Notes        { get; set; }   // Public notes
    public CaseStatus CaseStatus { get; set; }
    public string? FollowUpInstructions { get; set; }
    public string? TreatmentPlan        { get; set; }
    public string? DietInstructions     { get; set; }
    public string? HomeCareInstructions { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime UpdatedAt   { get; set; }
}

/// <summary>
/// Admin response: includes AllowedFood / RestrictedFood clinical summaries
/// but NEVER InternalNotes (BR-57: InternalNotes = Doctor/Owner only).
/// </summary>
public class AdminReportResponseDto : ReportResponseDto
{
    public string? AllowedFood    { get; set; }
    public string? RestrictedFood { get; set; }
}

/// <summary>
/// Doctor/Owner response: includes InternalNotes (BR-57) and all clinical detail fields.
/// </summary>
public class DoctorReportResponseDto : AdminReportResponseDto
{
    public string? InternalNotes { get; set; }  // BR-57: Doctor and Owner ONLY
}

/// <summary>Report timeline comparison result.</summary>
public class ReportCompareResponseDto
{
    public ReportResponseDto         EarlierReport          { get; set; } = null!;
    public ReportResponseDto         LaterReport            { get; set; } = null!;
    public IEnumerable<ToothDiffDto> ToothDiffs             { get; set; } = [];
    public int                       ChangedCount           { get; set; }
    public int                       ImprovedCount          { get; set; }
    public int                       WorsenedCount          { get; set; }
    public string                    DiagnosisChangeSummary { get; set; } = string.Empty;
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
