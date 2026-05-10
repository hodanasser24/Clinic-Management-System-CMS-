using DCMS.Domain.Entities;

namespace DCMS.Domain.Interfaces.Repositories;

public interface IDoctorRepository : IGenericRepository<Doctor>
{
    Task<Doctor?> GetByEmailAsync(string email, CancellationToken ct = default);
}
