using System;
using System.Collections.Generic;

namespace DCMS.Domain.Entities;

public class Patient : User
{
    public DateTime DateOfBirth { get; set; }
    public string? MedicalHistory { get; set; }

    // Navigation Properties
    public virtual ICollection<Appointment> Appointments { get; set; }
    public virtual ICollection<Report> Reports { get; set; }
    public virtual ICollection<ContactMessage> ContactMessages { get; set; }
    
    public virtual DentalChart? DentalChart { get; set; }

    public Patient()
    {
        Appointments = new HashSet<Appointment>();
        Reports = new HashSet<Report>();
        ContactMessages = new HashSet<ContactMessage>();
    }
}
