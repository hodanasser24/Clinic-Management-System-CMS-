using DCMS.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace DCMS.Domain.Interfaces.Repositories;

public interface IPrescriptionRepository : IGenericRepository<Prescription>
{
    Task<Prescription?> GetByIdWithItemsAsync(int id, CancellationToken ct = default);
    Task<Prescription?> GetByReportIdWithItemsAsync(int reportId, CancellationToken ct = default);
    Task<PagedResult<Prescription>> GetByPatientWithDetailsAsync(int patientId, int page, int pageSize, int? doctorId = null, CancellationToken ct = default);
}
