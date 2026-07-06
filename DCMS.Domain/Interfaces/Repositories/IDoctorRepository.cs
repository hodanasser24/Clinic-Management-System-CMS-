using DCMS.Domain.Entities;

namespace DCMS.Domain.Interfaces.Repositories;

public interface IDoctorRepository : IGenericRepository<Doctor>
{
    Task<Doctor?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Doctor?> GetDoctorWithScheduleAsync(int doctorId, DayOfWeek day, CancellationToken ct = default);
}
