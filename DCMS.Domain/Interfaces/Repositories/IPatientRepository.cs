using DCMS.Domain.Entities;

namespace DCMS.Domain.Interfaces.Repositories;

public interface IPatientRepository : IGenericRepository<Patient>
{
    Task<Patient?> GetByEmailAsync(string email, CancellationToken ct = default);
}
