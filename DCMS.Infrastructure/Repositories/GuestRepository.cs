using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces.Repositories;
using DCMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DCMS.Infrastructure.Repositories;

public class GuestRepository : GenericRepository<Guest>, IGuestRepository
{
    public GuestRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Guest?> GetBySessionIdAsync(string sessionId, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(g => g.SessionId == sessionId, ct);
}
