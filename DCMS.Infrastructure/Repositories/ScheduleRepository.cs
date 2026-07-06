using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces.Repositories;
using DCMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DCMS.Infrastructure.Repositories;

public class ScheduleRepository : GenericRepository<Schedule>, IScheduleRepository
{
    public ScheduleRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PagedResult<Schedule>> GetByDoctorAsync(int doctorId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.Where(s => s.DoctorId == doctorId);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Schedule>(items, total, page, pageSize);
    }

    public async Task<PagedResult<Schedule>> GetByBranchAsync(int branchId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.Where(s => s.BranchId == branchId);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Schedule>(items, total, page, pageSize);
    }

    public async Task<Schedule?> GetByDoctorBranchDayAsync(int doctorId, int branchId, DayOfWeek dayOfWeek, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.BranchId == branchId && s.DayOfWeek == dayOfWeek, ct);
}
