using System.Linq.Expressions;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces.Repositories;
using DCMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DCMS.Infrastructure.Repositories;

public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(ApplicationDbContext context) : base(context) { }

    // ── Basic queries (no nav props — used for conflict checks etc.) ───────────

    public async Task<IEnumerable<Appointment>> GetByDateAsync(
        DateOnly date, CancellationToken ct = default)
        => await _dbSet
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Branch)
            .Include(a => a.Service)
            .Where(a => a.Date == date)
            .ToListAsync(ct);

    public async Task<IEnumerable<Appointment>> GetByDoctorAndDateAsync(
        int doctorId, DateOnly date, CancellationToken ct = default)
        => await _dbSet
            .Where(a => a.DoctorId == doctorId && a.Date == date)
            .ToListAsync(ct);

    public async Task<bool> HasConflictAsync(
        int doctorId, DateOnly date, TimeOnly startTime,
        int? excludeAppointmentId = null, CancellationToken ct = default)
    {
        var query = _dbSet.Where(a =>
            a.DoctorId == doctorId &&
            a.Date     == date     &&
            a.StartTime == startTime &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.Rejected);

        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);

        return await query.AnyAsync(ct);
    }

    // ── Basic paged (legacy — no nav props) ────────────────────────────────────

    public async Task<PagedResult<Appointment>> GetByPatientAsync(
        int patientId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.Where(a => a.PatientId == patientId)
                          .OrderByDescending(a => a.Date).ThenByDescending(a => a.StartTime);
        return await ToPagedAsync(query, page, pageSize, ct);
    }

    public async Task<PagedResult<Appointment>> GetByDoctorAsync(
        int doctorId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.Where(a => a.DoctorId == doctorId)
                          .OrderByDescending(a => a.Date).ThenByDescending(a => a.StartTime);
        return await ToPagedAsync(query, page, pageSize, ct);
    }

    // ── Detail queries: eagerly load all navigation properties ─────────────────

    public async Task<Appointment?> GetByIdWithDetailsAsync(
        int id, CancellationToken ct = default)
        => await WithDetails()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<PagedResult<Appointment>> GetPagedWithDetailsAsync(
        int page, int pageSize,
        Expression<Func<Appointment, bool>>? predicate = null,
        string? sortBy = null, bool sortDescending = true,
        CancellationToken ct = default)
    {
        var query = WithDetails();
        if (predicate != null) query = query.Where(predicate);

        // Sorting logic
        if (string.Equals(sortBy, "CreatedAt", StringComparison.OrdinalIgnoreCase))
        {
            query = sortDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt);
        }
        else if (string.Equals(sortBy, "Date", StringComparison.OrdinalIgnoreCase))
        {
            query = sortDescending
                ? query.OrderByDescending(a => a.Date).ThenByDescending(a => a.StartTime)
                : query.OrderBy(a => a.Date).ThenBy(a => a.StartTime);
        }
        else
        {
            // Default sort (original behavior)
            query = query.OrderByDescending(a => a.Date).ThenBy(a => a.StartTime);
        }

        return await ToPagedAsync(query, page, pageSize, ct);
    }

    public async Task<PagedResult<Appointment>> GetByPatientWithDetailsAsync(
        int patientId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = WithDetails()
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.Date).ThenBy(a => a.StartTime);
        return await ToPagedAsync(query, page, pageSize, ct);
    }

    public async Task<PagedResult<Appointment>> GetByDoctorWithDetailsAsync(
        int doctorId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = WithDetails()
            .Where(a => a.DoctorId == doctorId)
            .OrderByDescending(a => a.Date).ThenBy(a => a.StartTime);
        return await ToPagedAsync(query, page, pageSize, ct);
    }

    public async Task<PagedResult<Appointment>> GetUpcomingByPatientAsync(
        int patientId, int page, int pageSize, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = WithDetails()
            .Where(a => a.PatientId == patientId &&
                        a.Date      >= today &&
                        (a.Status == AppointmentStatus.Pending ||
                         a.Status == AppointmentStatus.Confirmed))
            .OrderBy(a => a.Date).ThenBy(a => a.StartTime);
        return await ToPagedAsync(query, page, pageSize, ct);
    }

    public async Task<PagedResult<Appointment>> GetHistoryByPatientAsync(
        int patientId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = WithDetails()
            .Where(a => a.PatientId == patientId &&
                        (a.Status == AppointmentStatus.Completed  ||
                         a.Status == AppointmentStatus.Cancelled  ||
                         a.Status == AppointmentStatus.Rejected))
            .OrderByDescending(a => a.Date).ThenByDescending(a => a.StartTime);
        return await ToPagedAsync(query, page, pageSize, ct);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    /// <summary>Base query that eagerly loads all navigation properties needed by DTOs.</summary>
    private IQueryable<Appointment> WithDetails()
        => _dbSet
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Branch)
            .Include(a => a.Service)
            .Include(a => a.ConfirmedByAdmin)
            .AsNoTracking();

    private static async Task<PagedResult<Appointment>> ToPagedAsync(
        IQueryable<Appointment> query, int page, int pageSize, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return new PagedResult<Appointment>(items, total, page, pageSize);
    }
}
