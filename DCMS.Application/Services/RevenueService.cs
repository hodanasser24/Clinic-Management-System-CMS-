using DCMS.Application.DTOs.Revenue;
using DCMS.Application.Interfaces;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;

namespace DCMS.Application.Services;

public class RevenueService : IRevenueService
{
    private readonly IUnitOfWork _uow;

    public RevenueService(IUnitOfWork uow) => _uow = uow;

    public async Task<RevenueSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        // Only completed appointments generate revenue
        var all = await _uow.Appointments.GetAllAsync(ct);
        var completed = all
            .Where(a => a.Status == AppointmentStatus.Completed && a.Service != null)
            .ToList();

        var totalRevenue = completed.Sum(a => a.Service!.Price);
        var todayRevenue = completed.Where(a => a.Date == today).Sum(a => a.Service!.Price);
        var weekRevenue = completed.Where(a => a.Date >= weekStart).Sum(a => a.Service!.Price);
        var monthRevenue = completed.Where(a => a.Date >= monthStart).Sum(a => a.Service!.Price);

        var byBranch = completed
            .GroupBy(a => new { a.BranchId, BranchName = a.Branch?.Name ?? string.Empty })
            .Select(g => new RevenueBranchBreakdownDto
            {
                BranchId = g.Key.BranchId,
                BranchName = g.Key.BranchName,
                Revenue = g.Sum(a => a.Service!.Price),
                AppointmentCount = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        var byService = completed
            .GroupBy(a => new { a.ServiceId, ServiceName = a.Service?.Name ?? string.Empty })
            .Select(g => new RevenueServiceBreakdownDto
            {
                ServiceId = g.Key.ServiceId,
                ServiceName = g.Key.ServiceName,
                Revenue = g.Sum(a => a.Service!.Price),
                AppointmentCount = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        return new RevenueSummaryDto
        {
            TotalRevenue = totalRevenue,
            TodayRevenue = todayRevenue,
            WeekRevenue = weekRevenue,
            MonthRevenue = monthRevenue,
            CompletedAppointmentsTotal = completed.Count,
            ByBranch = byBranch,
            ByService = byService
        };
    }

    public async Task<RevenuePeriodDto> GetByPeriodAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var all = await _uow.Appointments.GetAllAsync(ct);
        var completed = all
            .Where(a => a.Status == AppointmentStatus.Completed
                        && a.Date >= from && a.Date <= to
                        && a.Service != null)
            .ToList();

        var daily = new List<RevenueDailyDto>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            var dayItems = completed.Where(a => a.Date == d).ToList();
            daily.Add(new RevenueDailyDto
            {
                Date = d,
                Revenue = dayItems.Sum(a => a.Service!.Price),
                AppointmentCount = dayItems.Count
            });
        }

        return new RevenuePeriodDto
        {
            From = from,
            To = to,
            TotalRevenue = completed.Sum(a => a.Service!.Price),
            CompletedAppointments = completed.Count,
            DailyBreakdown = daily
        };
    }
}
