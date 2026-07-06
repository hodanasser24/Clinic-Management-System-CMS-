using System.Collections.Generic;
using DCMS.Domain.Common;

namespace DCMS.Domain.Entities;

public class Service : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<Appointment> Appointments { get; set; }
    public virtual ICollection<OfferDiscount> OfferDiscounts { get; set; }

    public Service()
    {
        Appointments = new HashSet<Appointment>();
        OfferDiscounts = new HashSet<OfferDiscount>();
    }
}
