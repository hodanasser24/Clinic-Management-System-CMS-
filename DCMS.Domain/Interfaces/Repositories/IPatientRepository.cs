using DCMS.Domain.Entities;

namespace DCMS.Domain.Interfaces.Repositories;

public interface IPatientRepository : IGenericRepository<Patient>
{
    Task<Patient?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<PagedResult<Patient>> GetQueriedPagedAsync(int page, int pageSize, string? name, string? phone, int? id, int? branchId, int? serviceId, string? sortBy, bool sortDescending, CancellationToken ct = default);
}
