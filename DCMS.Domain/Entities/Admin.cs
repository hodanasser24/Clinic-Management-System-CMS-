using System.Collections.Generic;

namespace DCMS.Domain.Entities;

public class Admin : User
{
    // Admin-Only Navigation Properties for Submitted Requests
    public virtual ICollection<ScheduleChangeRequest> ScheduleChangeRequests { get; set; }
    public virtual ICollection<ServiceModificationRequest> ServiceModificationRequests { get; set; }
    public virtual ICollection<FAQModificationRequest> FAQModificationRequests { get; set; }
    public virtual ICollection<OfferDiscountModificationRequest> OfferDiscountModificationRequests { get; set; }
    public virtual ICollection<BranchModificationRequest> BranchModificationRequests { get; set; }

    public Admin()
    {
        ScheduleChangeRequests = new HashSet<ScheduleChangeRequest>();
        ServiceModificationRequests = new HashSet<ServiceModificationRequest>();
        FAQModificationRequests = new HashSet<FAQModificationRequest>();
        OfferDiscountModificationRequests = new HashSet<OfferDiscountModificationRequest>();
        BranchModificationRequests = new HashSet<BranchModificationRequest>();
    }
}
