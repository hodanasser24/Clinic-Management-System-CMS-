using DCMS.Application.DTOs.Reports;
using DCMS.Application.DTOs.Common;
using DCMS.Domain.Enums;

namespace DCMS.Application.Interfaces;

public interface IReportService
{
    Task<ReportResponseDto>              GetByIdAsync(int id, UserRole callerRole, CancellationToken ct = default);
    Task<PagedResultDto<ReportResponseDto>> GetByPatientAsync(int patientId, UserRole callerRole, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResultDto<ReportResponseDto>> GetByDoctorAsync(int doctorId, int page, int pageSize, CancellationToken ct = default);
    Task<ReportResponseDto>              CreateAsync(CreateReportRequestDto dto, CancellationToken ct = default);
    Task<ReportResponseDto>              UpdateAsync(int id, UpdateReportRequestDto dto, int requestingDoctorId, CancellationToken ct = default);

    /// <summary>
    /// Produces a clinical timeline diff between two reports for the same patient.
    /// Returns tooth status changes between the two report dates.
    /// </summary>
    Task<ReportCompareResponseDto> CompareAsync(int reportId1, int reportId2, CancellationToken ct = default);
}
