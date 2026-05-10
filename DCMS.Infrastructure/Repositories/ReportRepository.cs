using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces.Repositories;
using DCMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DCMS.Infrastructure.Repositories;

public class ReportRepository : GenericRepository<Report>, IReportRepository
{
    public ReportRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PagedResult<Report>> GetByPatientAsync(int patientId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.Where(r => r.PatientId == patientId);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Report>(items, total, page, pageSize);
    }

    public async Task<PagedResult<Report>> GetByDoctorAsync(int doctorId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.Where(r => r.DoctorId == doctorId);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Report>(items, total, page, pageSize);
    }

    public async Task<Report?> GetByAppointmentIdAsync(int appointmentId, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(r => r.AppointmentId == appointmentId, ct);
}
