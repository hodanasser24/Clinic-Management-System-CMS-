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
}
