using System.Linq.Expressions;
using DCMS.Domain.Entities;

namespace DCMS.Domain.Interfaces.Repositories;

public interface IAppointmentRepository : IGenericRepository<Appointment>
{
    Task<IEnumerable<Appointment>> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task<IEnumerable<Appointment>> GetByDoctorAndDateAsync(int doctorId, DateOnly date, CancellationToken ct = default);
    Task<PagedResult<Appointment>> GetByPatientAsync(int patientId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Appointment>> GetByDoctorAsync(int doctorId, int page, int pageSize, CancellationToken ct = default);
    Task<bool> HasConflictAsync(int doctorId, DateOnly date, TimeOnly startTime, int? excludeAppointmentId = null, CancellationToken ct = default);

    // Detail queries
    Task<Appointment?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<PagedResult<Appointment>> GetPagedWithDetailsAsync(int page, int pageSize, Expression<Func<Appointment, bool>>? predicate = null, CancellationToken ct = default);
    Task<PagedResult<Appointment>> GetByPatientWithDetailsAsync(int patientId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Appointment>> GetByDoctorWithDetailsAsync(int doctorId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Appointment>> GetUpcomingByPatientAsync(int patientId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Appointment>> GetHistoryByPatientAsync(int patientId, int page, int pageSize, CancellationToken ct = default);
}
