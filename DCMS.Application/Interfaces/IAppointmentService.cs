using DCMS.Application.DTOs.Appointments;
using DCMS.Application.DTOs.Common;

namespace DCMS.Application.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentResponseDto>              GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResultDto<AppointmentSummaryDto>> GetAllAsync(AppointmentQueryDto queryDto, CancellationToken ct = default);
    Task<PagedResultDto<AppointmentSummaryDto>> GetByPatientAsync(int patientId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResultDto<AppointmentSummaryDto>> GetByDoctorAsync(int doctorId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResultDto<AppointmentSummaryDto>> GetUrgentAsync(int page, int pageSize, CancellationToken ct = default);
    Task<PagedResultDto<AppointmentSummaryDto>> GetUrgentByDateRangeAsync(DateOnly from, DateOnly to, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Upcoming (Pending/Confirmed) appointments for a patient, date ≥ today.</summary>
    Task<PagedResultDto<AppointmentSummaryDto>> GetUpcomingByPatientAsync(int patientId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Past (Completed/Cancelled/Rejected) appointments for a patient — visit history.</summary>
    Task<PagedResultDto<AppointmentSummaryDto>> GetHistoryByPatientAsync(int patientId, int page, int pageSize, CancellationToken ct = default);

    Task<AppointmentResponseDto> BookAsync(AppointmentRequestDto dto, CancellationToken ct = default);
    Task<AppointmentResponseDto> RescheduleAsync(int id, int requestingUserId, RescheduleAppointmentRequestDto dto, CancellationToken ct = default);
    Task<AppointmentResponseDto> ConfirmAsync(int id, ConfirmAppointmentRequestDto dto, CancellationToken ct = default);
    Task<AppointmentResponseDto> RejectAsync(int id, RejectAppointmentRequestDto dto, CancellationToken ct = default);
    Task<AppointmentResponseDto> CancelAsync(int id, int requestingUserId, CancelAppointmentRequestDto dto, CancellationToken ct = default);
    Task<AppointmentResponseDto> MarkUrgentAsync(int id, MarkUrgentRequestDto dto, CancellationToken ct = default);
    Task<AppointmentResponseDto> UnmarkUrgentAsync(int id, int requestingDoctorId, CancellationToken ct = default);
    Task<AppointmentResponseDto> MarkAttendanceAsync(int id, MarkAttendanceRequestDto dto, CancellationToken ct = default);
}
