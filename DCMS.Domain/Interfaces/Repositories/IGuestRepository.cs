using DCMS.Domain.Entities;

namespace DCMS.Domain.Interfaces.Repositories;

public interface IGuestRepository : IGenericRepository<Guest>
{
    Task<Guest?> GetBySessionIdAsync(string sessionId, CancellationToken ct = default);
}
