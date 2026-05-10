using DCMS.Domain.Entities;

namespace DCMS.Domain.Interfaces.Repositories;

public interface IReportRepository : IGenericRepository<Report>
{
    Task<PagedResult<Report>> GetByPatientAsync(int patientId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Report>> GetByDoctorAsync(int doctorId, int page, int pageSize, CancellationToken ct = default);
    Task<Report?> GetByAppointmentIdAsync(int appointmentId, CancellationToken ct = default);
}
