using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces.Repositories;
using DCMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DCMS.Infrastructure.Repositories;

public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
{
    public DoctorRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Doctor?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(d => d.Email == email, ct);
}
