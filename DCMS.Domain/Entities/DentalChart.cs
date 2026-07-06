// File: DCMS.Domain/Entities/DentalChart.cs
using System.Collections.Generic;
using DCMS.Domain.Common;

namespace DCMS.Domain.Entities;

public class DentalChart : BaseEntity
{
    public int PatientId { get; set; }
    public string? Notes { get; set; }
    public DateTime LastUpdated { get; set; }

    // Navigation Properties
    public virtual Patient Patient { get; set; } = null!;
    public virtual ICollection<ToothRecord> ToothRecords { get; set; }

    public DentalChart()
    {
        ToothRecords = new HashSet<ToothRecord>();
    }
}
