using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

/// <summary>
/// Class Specification defines both Notes (public) and InternalNotes (doctor-only, BR-57).
/// Previous implementation had only InternalNotes; Notes is now restored.
/// </summary>
public class Report : BaseEntity
{
    public int PatientId     { get; set; }
    public int DoctorId      { get; set; }
    public int AppointmentId { get; set; }

    public string  Diagnosis     { get; set; } = string.Empty;
    public string? Treatment     { get; set; }

    // Public notes — visible to all authorised roles including Patient
    public string? Notes         { get; set; }

    // BR-57: Doctor and Owner only — NEVER exposed to Patient or Admin
    public string? InternalNotes { get; set; }

    // Extended clinical fields from Class Specification
    public string? FollowUpInstructions  { get; set; }
    public string? TreatmentPlan         { get; set; }
    public string? DietInstructions      { get; set; }
    public string? AllowedFood           { get; set; }
    public string? RestrictedFood        { get; set; }
    public string? HomeCareInstructions  { get; set; }

    public CaseStatus CaseStatus { get; set; } = CaseStatus.Completed;

    // Navigation Properties
    public virtual Patient     Patient     { get; set; } = null!;
    public virtual Doctor      Doctor      { get; set; } = null!;
    public virtual Appointment Appointment { get; set; } = null!;
    public virtual Prescription? Prescription { get; set; }
}
