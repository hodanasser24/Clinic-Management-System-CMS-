using System.Collections.Generic;

namespace DCMS.Domain.Entities;

public class Doctor : User
{
    public string Specialization { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public int ExperienceYears { get; set; }

    // Navigation Properties
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
