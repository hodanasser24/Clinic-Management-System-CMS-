using DCMS.Application.DTOs.ModificationRequests;
using DCMS.Application.DTOs.Common;

namespace DCMS.Application.Interfaces;

public interface IModificationRequestService
{
    // Service modification
    Task<ServiceModificationRequestResponseDto> CreateServiceRequestAsync(CreateServiceModificationRequestDto dto, CancellationToken ct = default);
    Task<ServiceModificationRequestResponseDto> ApproveRejectServiceRequestAsync(int requestId, ApproveRejectModificationRequestDto dto, int ownerId, CancellationToken ct = default);
    Task<PagedResultDto<ServiceModificationRequestResponseDto>> GetServiceRequestsAsync(int page, int pageSize, CancellationToken ct = default);

    // FAQ modification
    Task<FAQModificationRequestResponseDto> CreateFAQRequestAsync(CreateFAQModificationRequestDto dto, CancellationToken ct = default);
    Task<FAQModificationRequestResponseDto> ApproveRejectFAQRequestAsync(int requestId, ApproveRejectModificationRequestDto dto, int ownerId, CancellationToken ct = default);
    Task<PagedResultDto<FAQModificationRequestResponseDto>> GetFAQRequestsAsync(int page, int pageSize, CancellationToken ct = default);

    // Offer modification
    Task<OfferDiscountModificationRequestResponseDto> CreateOfferRequestAsync(CreateOfferDiscountModificationRequestDto dto, CancellationToken ct = default);
    Task<OfferDiscountModificationRequestResponseDto> ApproveRejectOfferRequestAsync(int requestId, ApproveRejectModificationRequestDto dto, int ownerId, CancellationToken ct = default);
    Task<PagedResultDto<OfferDiscountModificationRequestResponseDto>> GetOfferRequestsAsync(int page, int pageSize, CancellationToken ct = default);

    // Branch modification
    Task<BranchModificationRequestResponseDto> CreateBranchRequestAsync(CreateBranchModificationRequestDto dto, CancellationToken ct = default);
    Task<BranchModificationRequestResponseDto> ApproveRejectBranchRequestAsync(int requestId, ApproveRejectModificationRequestDto dto, int ownerId, CancellationToken ct = default);
    Task<PagedResultDto<BranchModificationRequestResponseDto>> GetBranchRequestsAsync(int page, int pageSize, CancellationToken ct = default);
}
