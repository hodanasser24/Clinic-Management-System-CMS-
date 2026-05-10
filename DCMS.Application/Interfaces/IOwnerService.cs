using DCMS.Application.DTOs.Owner;
using DCMS.Application.DTOs.Common;
using DCMS.Application.DTOs.Schedules;

namespace DCMS.Application.Interfaces;

public interface IOwnerService
{
    // Account management
    Task<DoctorAccountResponseDto> CreateDoctorAccountAsync(CreateDoctorAccountRequestDto dto, CancellationToken ct = default);
    Task<AccountResponseDto> CreateAdminAccountAsync(CreateAdminAccountRequestDto dto, CancellationToken ct = default);
    Task<AccountResponseDto> DeactivateAccountAsync(int userId, DeactivateAccountRequestDto dto, CancellationToken ct = default);
    Task<AccountResponseDto> ReactivateAccountAsync(int userId, CancellationToken ct = default);
    Task<PagedResultDto<DoctorAccountResponseDto>> GetAllDoctorsAsync(int page, int pageSize, CancellationToken ct = default);
    Task<PagedResultDto<AccountResponseDto>> GetAllAdminsAsync(int page, int pageSize, CancellationToken ct = default);

    // Offer management — BR-58: only Owner may activate/deactivate
    Task<OfferStatusResponseDto> ActivateOfferAsync(int offerId, CancellationToken ct = default);
    Task<OfferStatusResponseDto> DeactivateOfferAsync(int offerId, CancellationToken ct = default);
    Task<PagedResultDto<OfferStatusResponseDto>> GetAllOffersAsync(int page, int pageSize, CancellationToken ct = default);

    // Doctor profile management
    Task<DoctorAccountResponseDto> UpdateDoctorProfileAsync(int doctorId, UpdateDoctorProfileRequestDto dto, CancellationToken ct = default);
    Task<DoctorAccountResponseDto> UpdateDoctorPhotoAsync(int doctorId, UpdateDoctorPhotoRequestDto dto, CancellationToken ct = default);

    // Schedule change request approvals — BR-6
    Task<PagedResultDto<ScheduleChangeRequestResponseDto>> GetPendingScheduleRequestsAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ScheduleChangeRequestResponseDto> ApproveScheduleRequestAsync(int requestId, int ownerId, CancellationToken ct = default);
    Task<ScheduleChangeRequestResponseDto> RejectScheduleRequestAsync(int requestId, int ownerId, string? reason, CancellationToken ct = default);
}
