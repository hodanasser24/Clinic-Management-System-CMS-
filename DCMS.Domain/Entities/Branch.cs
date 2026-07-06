using System.Collections.Generic;
using DCMS.Domain.Common;

namespace DCMS.Domain.Entities;

public class Branch : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;  // Class Spec: NOT NULL
    public string? WorkingHours { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<Appointment> Appointments { get; set; }
    public virtual ICollection<Schedule> Schedules { get; set; }
    public virtual ICollection<OfferDiscount> OfferDiscounts { get; set; }

    public Branch()
    {
        Appointments = new HashSet<Appointment>();
        Schedules = new HashSet<Schedule>();
        OfferDiscounts = new HashSet<OfferDiscount>();
    }
}
