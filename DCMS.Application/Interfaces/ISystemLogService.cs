using DCMS.Application.DTOs.Common;
using DCMS.Application.DTOs.SystemLogs;

namespace DCMS.Application.Interfaces;

public interface ISystemLogService
{
    /// <summary>Owner: full access to all logs with optional filters.</summary>
    Task<PagedResultDto<SystemLogResponseDto>> GetAllAsync(SystemLogFilterDto filter, int page, int pageSize, CancellationToken ct = default);
    /// <summary>Admin: restricted to schedule-change logs only.</summary>
    Task<PagedResultDto<SystemLogResponseDto>> GetScheduleLogsAsync(int page, int pageSize, CancellationToken ct = default);
    Task<SystemLogResponseDto> GetByIdAsync(int id, CancellationToken ct = default);
}
