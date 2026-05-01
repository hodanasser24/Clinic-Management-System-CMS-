using System.Collections.Generic;

namespace DCMS.Domain.Entities;

public class Owner : Doctor
{
    // Owner-Only Navigation Properties for Approval Workflows
    public virtual ICollection<ScheduleChangeRequest> ApprovedScheduleChanges { get; set; }
    public virtual ICollection<ServiceModificationRequest> ApprovedServiceRequests { get; set; }
    public virtual ICollection<FAQModificationRequest> ApprovedFAQRequests { get; set; }
    public virtual ICollection<OfferDiscountModificationRequest> ApprovedOfferRequests { get; set; }
    public virtual ICollection<BranchModificationRequest> ApprovedBranchRequests { get; set; }

    public Owner()
    {
        ApprovedScheduleChanges = new HashSet<ScheduleChangeRequest>();
        ApprovedServiceRequests = new HashSet<ServiceModificationRequest>();
        ApprovedFAQRequests = new HashSet<FAQModificationRequest>();
        ApprovedOfferRequests = new HashSet<OfferDiscountModificationRequest>();
        ApprovedBranchRequests = new HashSet<BranchModificationRequest>();
    }
}
