using DCMS.Domain.Entities;

namespace DCMS.Domain.Interfaces.Repositories;

public interface IAdminRepository : IGenericRepository<Admin>
{
    Task<Admin?> GetByEmailAsync(string email, CancellationToken ct = default);
}
