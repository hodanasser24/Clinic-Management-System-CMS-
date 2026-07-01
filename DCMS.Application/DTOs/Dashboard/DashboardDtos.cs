namespace DCMS.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalAppointmentsToday { get; set; }
    public int TotalAppointmentsThisWeek { get; set; }
    public int PendingAppointments { get; set; }
    public int ConfirmedAppointments { get; set; }
    public int UrgentAppointments { get; set; }
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int ActiveBranches { get; set; }
    public int ActiveServices { get; set; }
    public int UnresolvedContactMessages { get; set; }
    public int PendingModificationRequests { get; set; }
    // Revenue fields
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class DailyReportDto
{
    public DateOnly Date { get; set; }
    public int TotalAppointments { get; set; }
    public int Attended { get; set; }
    public int Absent { get; set; }
    public int Cancelled { get; set; }
    public int Rejected { get; set; }
    public int Completed { get; set; }
    public int UrgentCases { get; set; }
    public decimal Revenue { get; set; }
    public List<AppointmentSummaryExportDto> Appointments { get; set; } = new();
}

public class WeeklyReportDto
{
    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }
    public int TotalAppointments { get; set; }
    public int Attended { get; set; }
    public int Absent { get; set; }
    public int Cancelled { get; set; }
    public decimal WeekRevenue { get; set; }
    public List<DailyReportDto> DailyBreakdown { get; set; } = new();
}

public class AppointmentSummaryExportDto
{
    public int AppointmentId { get; set; }
    public string PatientName { get; set; } = null!;
    public string DoctorName { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public TimeOnly StartTime { get; set; }
    public string Status { get; set; } = null!;
    public bool IsUrgent { get; set; }
}

public class DoctorDailyDashboardDto
{
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    
    public int PatientsSeen { get; set; }
    public int ReportsCreated { get; set; }
    public int PrescriptionsCreated { get; set; }
    public int DentalChartsUpdated { get; set; }

    public List<DoctorActivityDto> ActivityTimeline { get; set; } = new();
}

public class DoctorActivityDto
{
    public DateTime Time { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
