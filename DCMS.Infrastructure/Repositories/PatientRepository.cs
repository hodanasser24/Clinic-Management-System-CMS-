using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces.Repositories;
using DCMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DCMS.Infrastructure.Repositories;

public class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    public PatientRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Patient?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(p => p.Email == email, ct);

    public async Task<PagedResult<Patient>> GetQueriedPagedAsync(
        int page, int pageSize,
        string? name, string? phone, int? id,
        int? branchId, int? serviceId,
        string? sortBy, bool sortDescending,
        CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking();

        if (id.HasValue)
            query = query.Where(p => p.Id == id.Value);

        if (!string.IsNullOrEmpty(name))
            query = query.Where(p => p.FullName.Contains(name));

        if (!string.IsNullOrEmpty(phone))
            query = query.Where(p => p.Phone != null && p.Phone.Contains(phone));

        if (branchId.HasValue)
            query = query.Where(p => p.Appointments.Any(a => a.BranchId == branchId.Value));

        if (serviceId.HasValue)
            query = query.Where(p => p.Appointments.Any(a => a.ServiceId == serviceId.Value));

        if (string.Equals(sortBy, "Name", StringComparison.OrdinalIgnoreCase))
        {
            query = sortDescending ? query.OrderByDescending(p => p.FullName) : query.OrderBy(p => p.FullName);
        }
        else
        {
            // Default: Registered (CreatedAt)
            query = sortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Patient>(items, total, page, pageSize);
    }
}
