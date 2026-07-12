using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces.Repositories;
using DCMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DCMS.Infrastructure.Repositories;

public class PrescriptionRepository : GenericRepository<Prescription>, IPrescriptionRepository
{
    public PrescriptionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Prescription?> GetByIdWithItemsAsync(int id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Prescription?> GetByReportIdWithItemsAsync(int reportId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.ReportId == reportId, ct);
    }

    public async Task<PagedResult<Prescription>> GetByPatientWithDetailsAsync(int patientId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet
            .Include(p => p.Items)
            .Include(p => p.Report)
            .Where(p => p.Report.PatientId == patientId)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking();

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        
        return new PagedResult<Prescription>(items, total, page, pageSize);
    }
}
