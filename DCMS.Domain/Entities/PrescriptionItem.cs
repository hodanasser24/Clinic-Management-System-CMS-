// File: DCMS.Domain/Entities/PrescriptionItem.cs
using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

public class PrescriptionItem : BaseEntity
{
    public int PrescriptionId { get; set; }

    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public MedicationRoute Route { get; set; } = MedicationRoute.Oral;
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Prescription Prescription { get; set; } = null!;
}
