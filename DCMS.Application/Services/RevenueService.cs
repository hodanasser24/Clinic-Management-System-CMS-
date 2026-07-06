using DCMS.Application.DTOs.Revenue;
using DCMS.Application.Interfaces;
using DCMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        // Query directly from Revenues table for accuracy and performance
        var allRevenues = await _uow.Revenues.FindAsync(r => true, ct);
        var list = allRevenues.ToList();

        var totalRevenue = list.Sum(r => r.Amount);
        var todayRevenue = list.Where(r => r.Date == today).Sum(r => r.Amount);
        var weekRevenue = list.Where(r => r.Date >= weekStart).Sum(r => r.Amount);
        var monthRevenue = list.Where(r => r.Date >= monthStart).Sum(r => r.Amount);

        // Branch Breakdown
        var byBranch = list
            .GroupBy(r => new { r.BranchId, BranchName = r.Branch?.Name ?? $"Branch #{r.BranchId}" })
            .Select(g => new RevenueBranchBreakdownDto
            {
                BranchId = g.Key.BranchId,
                BranchName = g.Key.BranchName,
                Revenue = g.Sum(r => r.Amount),
                AppointmentCount = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        // Service Breakdown
        var byService = list
            .GroupBy(r => new { r.ServiceId, ServiceName = r.Service?.Name ?? $"Service #{r.ServiceId}" })
            .Select(g => new RevenueServiceBreakdownDto
            {
                ServiceId = g.Key.ServiceId,
                ServiceName = g.Key.ServiceName,
                Revenue = g.Sum(r => r.Amount),
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
            CompletedAppointmentsTotal = list.Count,
            ByBranch = byBranch,
            ByService = byService
        };
    }

    public async Task<RevenuePeriodDto> GetByPeriodAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var revenues = await _uow.Revenues.FindAsync(r => r.Date >= from && r.Date <= to, ct);
        var list = revenues.ToList();

        var daily = new List<RevenueDailyDto>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            var dayItems = list.Where(r => r.Date == d).ToList();
            daily.Add(new RevenueDailyDto
            {
                Date = d,
                Revenue = dayItems.Sum(r => r.Amount),
                AppointmentCount = dayItems.Count
            });
        }

        return new RevenuePeriodDto
        {
            From = from,
            To = to,
            TotalRevenue = list.Sum(r => r.Amount),
            CompletedAppointments = list.Count,
            DailyBreakdown = daily
        };
    }
}
