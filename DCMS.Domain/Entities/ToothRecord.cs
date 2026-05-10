using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

public class ToothRecord : BaseEntity
{
    public int ChartId { get; set; }
    public int ToothNumber { get; set; }

    public ToothStatus ToothStatus { get; set; } = ToothStatus.Healthy;
    public TreatmentType? TreatmentType { get; set; }
    public string? Notes { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateOnly? TreatmentDate { get; set; }
    public int? LastUpdatedInReportId { get; set; }  // Class Spec: was missing

    public virtual DentalChart DentalChart { get; set; } = null!;
    public virtual Report? LastUpdatedInReport { get; set; }
}
