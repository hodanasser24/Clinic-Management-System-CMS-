using DCMS.Application.DTOs.Common;
using DCMS.Application.DTOs.SystemLogs;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Interfaces;

using AutoMapper;

namespace DCMS.Application.Services;

public class SystemLogService : ISystemLogService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public SystemLogService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    /// <summary>Owner: all logs, with optional filters.</summary>
    public async Task<PagedResultDto<SystemLogResponseDto>> GetAllAsync(SystemLogFilterDto filter, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.SystemLogs.GetPagedAsync(page, pageSize, log =>
            (filter.EntityType == null || log.EntityType == filter.EntityType) &&
            (filter.ActionType == null || log.ActionType.Contains(filter.ActionType)) &&
            (filter.From == null || log.Date >= filter.From) &&
            (filter.To == null || log.Date <= filter.To) &&
            (filter.UserId == null || log.UserId == filter.UserId),
            ct);

        return new PagedResultDto<SystemLogResponseDto>
        {
            Items = _mapper.Map<List<SystemLogResponseDto>>(paged.Items),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    /// <summary>Admin: only schedule-change related logs.</summary>
    public async Task<PagedResultDto<SystemLogResponseDto>> GetScheduleLogsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.SystemLogs.GetPagedAsync(page, pageSize,
            log => log.EntityType == "schedule" || log.EntityType == "schedulechangerequest",
            ct);

        return new PagedResultDto<SystemLogResponseDto>
        {
            Items = _mapper.Map<List<SystemLogResponseDto>>(paged.Items),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<SystemLogResponseDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var log = await _uow.SystemLogs.GetByIdAsync(id, ct);
        if (log == null) throw new NotFoundException($"System log {id} not found.");
        return _mapper.Map<SystemLogResponseDto>(log);
    }
}
