using DCMS.Domain.Common;

namespace DCMS.Domain.Entities;

public class Revenue : BaseEntity
{
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public int AppointmentId { get; set; }
    public int BranchId { get; set; }
    public int ServiceId { get; set; }
    public int PatientId { get; set; }

    // Navigation properties
    public virtual Appointment Appointment { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
    public virtual Patient Patient { get; set; } = null!;
}
