using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces.Repositories;
using DCMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DCMS.Infrastructure.Repositories;

public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Appointment>> GetByDateAsync(DateOnly date, CancellationToken ct = default)
        => await _dbSet.Where(a => a.Date == date).ToListAsync(ct);

    public async Task<IEnumerable<Appointment>> GetByDoctorAndDateAsync(int doctorId, DateOnly date, CancellationToken ct = default)
        => await _dbSet.Where(a => a.DoctorId == doctorId && a.Date == date).ToListAsync(ct);

    public async Task<PagedResult<Appointment>> GetByPatientAsync(int patientId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.Where(a => a.PatientId == patientId);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(a => a.Date)
                              .ThenByDescending(a => a.StartTime)
                              .Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync(ct);
        return new PagedResult<Appointment>(items, total, page, pageSize);
    }

    public async Task<PagedResult<Appointment>> GetByDoctorAsync(int doctorId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.Where(a => a.DoctorId == doctorId);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(a => a.Date)
                              .ThenByDescending(a => a.StartTime)
                              .Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync(ct);
        return new PagedResult<Appointment>(items, total, page, pageSize);
    }

    public async Task<bool> HasConflictAsync(int doctorId, DateOnly date, TimeOnly startTime, int? excludeAppointmentId = null, CancellationToken ct = default)
    {
        var query = _dbSet.Where(a => a.DoctorId == doctorId && a.Date == date && a.StartTime == startTime);
        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        
        return await query.AnyAsync(ct);
    }
}
