using DCMS.Application.DTOs.Schedules;
using DCMS.Application.DTOs.Common;

namespace DCMS.Application.Interfaces;

public interface IScheduleService
{
    Task<ScheduleResponseDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResultDto<ScheduleResponseDto>> GetByDoctorAsync(int doctorId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResultDto<ScheduleResponseDto>> GetByBranchAsync(int branchId, int page, int pageSize, CancellationToken ct = default);
    Task<List<AvailableSlotDto>> GetAvailableTimeSlotsAsync(int scheduleId, DateOnly date, CancellationToken ct = default);
    Task<ScheduleResponseDto> CreateAsync(CreateScheduleRequestDto dto, CancellationToken ct = default);
    Task<ScheduleResponseDto> UpdateAsync(int id, UpdateScheduleRequestDto dto, CancellationToken ct = default);
    Task<ScheduleChangeRequestResponseDto> SubmitChangeRequestAsync(CreateScheduleChangeRequestDto dto, CancellationToken ct = default);
    Task<ScheduleChangeRequestResponseDto> ApproveChangeRequestByDoctorAsync(int requestId, int doctorId, CancellationToken ct = default);
    Task<ScheduleChangeRequestResponseDto> ApproveChangeRequestByOwnerAsync(int requestId, int ownerId, CancellationToken ct = default);
    Task<ScheduleChangeRequestResponseDto> RejectChangeRequestAsync(int requestId, int rejectingUserId, CancellationToken ct = default);
}
