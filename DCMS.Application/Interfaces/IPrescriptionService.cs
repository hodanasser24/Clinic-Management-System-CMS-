using DCMS.Application.DTOs.Prescriptions;
using DCMS.Application.DTOs.Common;

namespace DCMS.Application.Interfaces;

public interface IPrescriptionService
{
    Task<PrescriptionResponseDto> GetByIdAsync(int id, int? callerId = null, CancellationToken ct = default);
    Task<PrescriptionResponseDto> GetByReportIdAsync(int reportId, int? callerId = null, CancellationToken ct = default);
    Task<PagedResultDto<PrescriptionResponseDto>> GetByPatientAsync(int patientId, int page, int pageSize, int? callerId = null, CancellationToken ct = default);
    Task<PrescriptionResponseDto> CreateAsync(CreatePrescriptionRequestDto dto, int requestingDoctorId, CancellationToken ct = default);
    Task<PrescriptionResponseDto> UpdateAsync(int id, int requestingDoctorId, CreatePrescriptionRequestDto dto, CancellationToken ct = default);
    Task<byte[]>                  ExportPdfAsync(int id, CancellationToken ct = default);
}
