using DCMS.Application.DTOs.Revenue;

namespace DCMS.Application.Interfaces;

public interface IRevenueService
{
    Task<RevenueSummaryDto> GetSummaryAsync(CancellationToken ct = default);
    Task<RevenuePeriodDto> GetByPeriodAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
}
