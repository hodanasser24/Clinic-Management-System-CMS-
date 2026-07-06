// File: DCMS.Domain/Entities/Prescription.cs
using System.Collections.Generic;
using DCMS.Domain.Common;

namespace DCMS.Domain.Entities;

public class Prescription : BaseEntity
{
    public int ReportId { get; set; }
    public string? GeneralInstructions { get; set; }

    // Navigation Properties
    public virtual Report Report { get; set; } = null!;
    public virtual ICollection<PrescriptionItem> Items { get; set; }

    public Prescription()
    {
        Items = new HashSet<PrescriptionItem>();
    }
}
