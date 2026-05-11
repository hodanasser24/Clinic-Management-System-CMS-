using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces.Repositories;
using DCMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DCMS.Infrastructure.Repositories;

public class ReportRepository : GenericRepository<Report>, IReportRepository
{
    public ReportRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PagedResult<Report>> GetByPatientAsync(
        int patientId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.Where(r => r.PatientId == patientId)
                          .OrderByDescending(r => r.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Report>(items, total, page, pageSize);
    }

    public async Task<PagedResult<Report>> GetByDoctorAsync(
        int doctorId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.Where(r => r.DoctorId == doctorId)
                          .OrderByDescending(r => r.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Report>(items, total, page, pageSize);
    }

    public async Task<Report?> GetByAppointmentIdAsync(
        int appointmentId, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(r => r.AppointmentId == appointmentId, ct);

    // ── Detail queries ─────────────────────────────────────────────────────────

    public async Task<Report?> GetByIdWithDetailsAsync(
        int id, CancellationToken ct = default)
        => await WithDetails()
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<PagedResult<Report>> GetByPatientWithDetailsAsync(
        int patientId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = WithDetails()
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Report>(items, total, page, pageSize);
    }

    public async Task<PagedResult<Report>> GetByDoctorWithDetailsAsync(
        int doctorId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = WithDetails()
            .Where(r => r.DoctorId == doctorId)
            .OrderByDescending(r => r.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Report>(items, total, page, pageSize);
    }

    private IQueryable<Report> WithDetails()
        => _dbSet
            .Include(r => r.Patient)
                .ThenInclude(p => p.DentalChart!)
                    .ThenInclude(dc => dc.ToothRecords)
            .Include(r => r.Doctor)
            .Include(r => r.Appointment)
            .Include(r => r.Prescription)
                .ThenInclude(p => p.Items)
            .AsNoTracking();
}
