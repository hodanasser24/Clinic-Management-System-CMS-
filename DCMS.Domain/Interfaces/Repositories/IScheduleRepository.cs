using DCMS.Domain.Entities;

namespace DCMS.Domain.Interfaces.Repositories;

public interface IScheduleRepository : IGenericRepository<Schedule>
{
    Task<PagedResult<Schedule>> GetByDoctorAsync(int doctorId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Schedule>> GetByBranchAsync(int branchId, int page, int pageSize, CancellationToken ct = default);
    Task<Schedule?> GetByDoctorBranchDayAsync(int doctorId, int branchId, DayOfWeek dayOfWeek, CancellationToken ct = default);
}
