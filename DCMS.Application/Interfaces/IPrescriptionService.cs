using DCMS.Application.DTOs.Prescriptions;

namespace DCMS.Application.Interfaces;

public interface IPrescriptionService
{
    Task<PrescriptionResponseDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PrescriptionResponseDto> GetByReportIdAsync(int reportId, CancellationToken ct = default);
    Task<PrescriptionResponseDto> CreateAsync(CreatePrescriptionRequestDto dto, CancellationToken ct = default);
    Task<PrescriptionResponseDto> UpdateAsync(int id, int requestingDoctorId, CreatePrescriptionRequestDto dto, CancellationToken ct = default);
    Task<byte[]>                  ExportPdfAsync(int id, CancellationToken ct = default);
}
