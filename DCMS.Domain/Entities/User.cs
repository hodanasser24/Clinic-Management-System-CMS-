using System.Collections.Generic;
using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

public abstract class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }

    public bool IsFirstLogin { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    // Navigation Properties
    public virtual ICollection<Notification> Notifications { get; set; }
    public virtual ICollection<SystemLog> SystemLogs { get; set; }

    protected User()
    {
        Notifications = new HashSet<Notification>();
        SystemLogs = new HashSet<SystemLog>();
    }
}

public class Patient : User
{
    public DateOnly DateOfBirth { get; set; }
    public string? MedicalHistory { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; }
    public virtual ICollection<Report> Reports { get; set; }
    public virtual DentalChart? DentalChart { get; set; }

    public Patient()
    {
        Appointments = new HashSet<Appointment>();
        Reports = new HashSet<Report>();
    }
}

public class Doctor : User
{
    public string Specialization { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public int ExperienceYears { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; }
    public virtual ICollection<Schedule> Schedules { get; set; }
    public virtual ICollection<Report> Reports { get; set; }
    public virtual ICollection<ScheduleChangeRequest> ScheduleChangeRequests { get; set; }

    public Doctor()
    {
        Appointments = new HashSet<Appointment>();
        Schedules = new HashSet<Schedule>();
        Reports = new HashSet<Report>();
        ScheduleChangeRequests = new HashSet<ScheduleChangeRequest>();
    }
}

public class Owner : Doctor
{
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

public class Admin : User
{
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

public class Guest : User
{
    public string SessionId { get; set; } = string.Empty;
    public virtual ICollection<ContactMessage> ContactMessages { get; set; }

    public Guest()
    {
        ContactMessages = new HashSet<ContactMessage>();
    }
}
