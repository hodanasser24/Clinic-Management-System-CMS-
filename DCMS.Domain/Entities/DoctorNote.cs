using DCMS.Domain.Common;

namespace DCMS.Domain.Entities;

public class DoctorNote : BaseEntity
{
    public int DoctorId { get; set; }
    public string Content { get; set; } = string.Empty;

    // Navigation Properties
    public virtual Doctor Doctor { get; set; } = null!;
}
