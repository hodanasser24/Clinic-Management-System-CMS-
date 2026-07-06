using DCMS.Application.DTOs.DentalChart;

namespace DCMS.Application.Interfaces;

public interface IDentalChartService
{
    Task<DentalChartResponseDto> GetByPatientIdAsync(int patientId, CancellationToken ct = default);
    Task<DentalChartResponseDto> UpdateChartNotesAsync(int patientId, UpdateDentalChartRequestDto dto, int requestingDoctorId, CancellationToken ct = default);
    Task<DentalChartResponseDto> UpsertToothRecordAsync(int patientId, UpsertToothRecordRequestDto dto, int requestingDoctorId, CancellationToken ct = default);
}
