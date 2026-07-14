using DCMS.Application.DTOs.Dashboard;

namespace DCMS.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(int? doctorId = null, CancellationToken ct = default);
    Task<DailyReportDto> GetDailyReportAsync(DateOnly date, int? doctorId = null, CancellationToken ct = default);
    Task<WeeklyReportDto> GetWeeklyReportAsync(DateOnly weekStart, int? doctorId = null, CancellationToken ct = default);
    Task<byte[]> ExportDailyReportAsCsvAsync(DateOnly date, int? doctorId = null, CancellationToken ct = default);
    Task<byte[]> ExportWeeklyReportAsCsvAsync(DateOnly weekStart, int? doctorId = null, CancellationToken ct = default);
    
    Task<DoctorDailyDashboardDto> GetDoctorDailyTrackingAsync(int doctorId, CancellationToken ct = default);
}
