using DCMS.Application.DTOs.Dashboard;

namespace DCMS.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default);
    Task<DailyReportDto> GetDailyReportAsync(DateOnly date, CancellationToken ct = default);
    Task<WeeklyReportDto> GetWeeklyReportAsync(DateOnly weekStart, CancellationToken ct = default);
    Task<byte[]> ExportDailyReportAsCsvAsync(DateOnly date, CancellationToken ct = default);
    Task<byte[]> ExportWeeklyReportAsCsvAsync(DateOnly weekStart, CancellationToken ct = default);
}
