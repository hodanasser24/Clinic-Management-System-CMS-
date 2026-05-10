namespace DCMS.Application.DTOs.Revenue;

public class RevenueSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public int CompletedAppointmentsTotal { get; set; }
    public List<RevenueBranchBreakdownDto> ByBranch { get; set; } = new();
    public List<RevenueServiceBreakdownDto> ByService { get; set; } = new();
}

public class RevenueBranchBreakdownDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public decimal Revenue { get; set; }
    public int AppointmentCount { get; set; }
}

public class RevenueServiceBreakdownDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public decimal Revenue { get; set; }
    public int AppointmentCount { get; set; }
}

public class RevenuePeriodDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public decimal TotalRevenue { get; set; }
    public int CompletedAppointments { get; set; }
    public List<RevenueDailyDto> DailyBreakdown { get; set; } = new();
}

public class RevenueDailyDto
{
    public DateOnly Date { get; set; }
    public decimal Revenue { get; set; }
    public int AppointmentCount { get; set; }
}
