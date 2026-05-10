using DCMS.Application.DTOs.Common;
using DCMS.Application.DTOs.ModificationRequests;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;

namespace DCMS.Application.Services;

public class ModificationRequestService : IModificationRequestService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;

    public ModificationRequestService(IUnitOfWork uow, INotificationService notificationService)
    {
        _uow = uow;
        _notificationService = notificationService;
    }

    // ─── Service Modification ────────────────────────────────

    public async Task<ServiceModificationRequestResponseDto> CreateServiceRequestAsync(CreateServiceModificationRequestDto dto, CancellationToken ct = default)
    {
        // BR-10: Admin submits the request
        var admin = await _uow.Admins.GetByIdAsync(dto.AdminId, ct);
        if (admin == null) throw new NotFoundException("Admin not found.");

        var request = new ServiceModificationRequest
        {
            AdminId = dto.AdminId,
            OwnerId = dto.OwnerId,
            ServiceId = dto.ServiceId,
            RequestType = dto.RequestType,
            Status = RequestStatus.Pending,
            ProposedName = dto.ProposedName,
            ProposedPrice = dto.ProposedPrice,
            ProposedDescription = dto.ProposedDescription
        };

        await _uow.ServiceModificationRequests.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            dto.OwnerId, NotificationType.ServiceModificationRequestSubmitted, NotificationPriority.Normal,
            "Service Modification Request", "A new service modification request awaits your review.",
            request.Id, "ServiceModificationRequest", ct);

        return await MapServiceRequestResponseAsync(request, ct);
    }

    public async Task<ServiceModificationRequestResponseDto> ApproveRejectServiceRequestAsync(int requestId, ApproveRejectModificationRequestDto dto, int ownerId, CancellationToken ct = default)
    {
        var request = await _uow.ServiceModificationRequests.GetByIdAsync(requestId, ct);
        if (request == null) throw new NotFoundException($"Service modification request {requestId} not found.");

        if (request.OwnerId != ownerId) throw new ForbiddenException("Only the designated Owner can approve/reject this request.");
        if (request.Status != RequestStatus.Pending) throw new BusinessRuleException("Request is no longer pending.");

        if (dto.Approve)
        {
            request.Status = RequestStatus.Approved;
            await ApplyServiceModificationAsync(request, ct);
        }
        else
        {
            request.Status = RequestStatus.Rejected;
        }

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            request.AdminId,
            dto.Approve ? NotificationType.ServiceModificationRequestApproved : NotificationType.ServiceModificationRequestRejected,
            NotificationPriority.Normal,
            dto.Approve ? "Request Approved" : "Request Rejected",
            dto.Approve ? "Your service modification request was approved." : "Your service modification request was rejected.",
            request.Id, "ServiceModificationRequest", ct);

        return await MapServiceRequestResponseAsync(request, ct);
    }

    public async Task<PagedResultDto<ServiceModificationRequestResponseDto>> GetServiceRequestsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.ServiceModificationRequests.GetPagedAsync(page, pageSize, ct: ct);
        var items = new List<ServiceModificationRequestResponseDto>();
        foreach (var r in paged.Items)
            items.Add(await MapServiceRequestResponseAsync(r, ct));

        return new PagedResultDto<ServiceModificationRequestResponseDto>
        {
            Items = items,
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    private async Task ApplyServiceModificationAsync(ServiceModificationRequest request, CancellationToken ct)
    {
        if (request.RequestType == RequestType.Add)
        {
            var service = new Service
            {
                Name = request.ProposedName!,
                Description = request.ProposedDescription,
                Price = request.ProposedPrice ?? 0,
                IsActive = true
            };
            await _uow.Services.AddAsync(service, ct);
        }
        else if (request.RequestType == RequestType.Update && request.ServiceId.HasValue)
        {
            var service = await _uow.Services.GetByIdAsync(request.ServiceId.Value, ct);
            if (service != null)
            {
                if (request.ProposedName != null) service.Name = request.ProposedName;
                if (request.ProposedPrice.HasValue) service.Price = request.ProposedPrice.Value;
                if (request.ProposedDescription != null) service.Description = request.ProposedDescription;
            }
        }
        else if (request.RequestType == RequestType.Delete && request.ServiceId.HasValue)
        {
            var service = await _uow.Services.GetByIdAsync(request.ServiceId.Value, ct);
            if (service != null) service.IsActive = false;
        }
    }

    private async Task<ServiceModificationRequestResponseDto> MapServiceRequestResponseAsync(ServiceModificationRequest r, CancellationToken ct)
    {
        var admin = await _uow.Admins.GetByIdAsync(r.AdminId, ct);
        var owner = await _uow.Doctors.GetByIdAsync(r.OwnerId, ct);
        return new ServiceModificationRequestResponseDto
        {
            Id = r.Id,
            AdminId = r.AdminId,
            AdminName = admin?.FullName ?? string.Empty,
            OwnerId = r.OwnerId,
            OwnerName = owner?.FullName ?? string.Empty,
            ServiceId = r.ServiceId,
            RequestType = r.RequestType,
            Status = r.Status,
            ProposedName = r.ProposedName,
            ProposedPrice = r.ProposedPrice,
            ProposedDescription = r.ProposedDescription,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }

    // ─── FAQ Modification ────────────────────────────────────

    public async Task<FAQModificationRequestResponseDto> CreateFAQRequestAsync(CreateFAQModificationRequestDto dto, CancellationToken ct = default)
    {
        var admin = await _uow.Admins.GetByIdAsync(dto.AdminId, ct);
        if (admin == null) throw new NotFoundException("Admin not found.");

        var request = new FAQModificationRequest
        {
            AdminId = dto.AdminId,
            OwnerId = dto.OwnerId,
            FAQId = dto.FAQId,
            RequestType = dto.RequestType,
            Status = RequestStatus.Pending,
            ProposedQuestion = dto.ProposedQuestion,
            ProposedAnswer = dto.ProposedAnswer
        };

        await _uow.FAQModificationRequests.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            dto.OwnerId, NotificationType.FAQModificationRequestSubmitted, NotificationPriority.Normal,
            "FAQ Modification Request", "A new FAQ modification request awaits your review.",
            request.Id, "FAQModificationRequest", ct);

        return await MapFAQRequestResponseAsync(request, ct);
    }

    public async Task<FAQModificationRequestResponseDto> ApproveRejectFAQRequestAsync(int requestId, ApproveRejectModificationRequestDto dto, int ownerId, CancellationToken ct = default)
    {
        var request = await _uow.FAQModificationRequests.GetByIdAsync(requestId, ct);
        if (request == null) throw new NotFoundException($"FAQ modification request {requestId} not found.");

        if (request.OwnerId != ownerId) throw new ForbiddenException("Only the designated Owner can approve/reject this request.");
        if (request.Status != RequestStatus.Pending) throw new BusinessRuleException("Request is no longer pending.");

        if (dto.Approve)
        {
            request.Status = RequestStatus.Approved;
            await ApplyFAQModificationAsync(request, ct);
        }
        else
        {
            request.Status = RequestStatus.Rejected;
        }

        await _uow.SaveChangesAsync(ct);
        return await MapFAQRequestResponseAsync(request, ct);
    }

    public async Task<PagedResultDto<FAQModificationRequestResponseDto>> GetFAQRequestsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.FAQModificationRequests.GetPagedAsync(page, pageSize, ct: ct);
        var items = new List<FAQModificationRequestResponseDto>();
        foreach (var r in paged.Items)
            items.Add(await MapFAQRequestResponseAsync(r, ct));

        return new PagedResultDto<FAQModificationRequestResponseDto>
        {
            Items = items,
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    private async Task ApplyFAQModificationAsync(FAQModificationRequest request, CancellationToken ct)
    {
        if (request.RequestType == RequestType.Add)
        {
            var faq = new FAQ
            {
                Question = request.ProposedQuestion!,
                Answer = request.ProposedAnswer!,
                IsActive = true,
                DisplayOrder = 0
            };
            await _uow.FAQs.AddAsync(faq, ct);
        }
        else if (request.RequestType == RequestType.Update && request.FAQId.HasValue)
        {
            var faq = await _uow.FAQs.GetByIdAsync(request.FAQId.Value, ct);
            if (faq != null)
            {
                if (request.ProposedQuestion != null) faq.Question = request.ProposedQuestion;
                if (request.ProposedAnswer != null) faq.Answer = request.ProposedAnswer;
            }
        }
        else if (request.RequestType == RequestType.Delete && request.FAQId.HasValue)
        {
            var faq = await _uow.FAQs.GetByIdAsync(request.FAQId.Value, ct);
            if (faq != null) faq.IsActive = false;
        }
    }

    private async Task<FAQModificationRequestResponseDto> MapFAQRequestResponseAsync(FAQModificationRequest r, CancellationToken ct)
    {
        var admin = await _uow.Admins.GetByIdAsync(r.AdminId, ct);
        var owner = await _uow.Doctors.GetByIdAsync(r.OwnerId, ct);
        return new FAQModificationRequestResponseDto
        {
            Id = r.Id,
            AdminId = r.AdminId,
            AdminName = admin?.FullName ?? string.Empty,
            OwnerId = r.OwnerId,
            OwnerName = owner?.FullName ?? string.Empty,
            FAQId = r.FAQId,
            RequestType = r.RequestType,
            Status = r.Status,
            ProposedQuestion = r.ProposedQuestion,
            ProposedAnswer = r.ProposedAnswer,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }

    // ─── Offer Modification ───────────────────────────────────

    public async Task<OfferDiscountModificationRequestResponseDto> CreateOfferRequestAsync(CreateOfferDiscountModificationRequestDto dto, CancellationToken ct = default)
    {
        var admin = await _uow.Admins.GetByIdAsync(dto.AdminId, ct);
        if (admin == null) throw new NotFoundException("Admin not found.");

        var request = new OfferDiscountModificationRequest
        {
            AdminId = dto.AdminId,
            OwnerId = dto.OwnerId,
            OfferId = dto.OfferId,
            RequestType = dto.RequestType,
            Status = RequestStatus.Pending,
            ProposedTitle = dto.ProposedTitle,
            ProposedDiscountPercentage = dto.ProposedDiscountPercentage,
            ProposedStartDate = dto.ProposedStartDate,
            ProposedEndDate = dto.ProposedEndDate
        };

        await _uow.OfferDiscountModificationRequests.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            dto.OwnerId, NotificationType.OfferModificationRequestSubmitted, NotificationPriority.Normal,
            "Offer Modification Request", "A new offer modification request awaits your review.",
            request.Id, "OfferDiscountModificationRequest", ct);

        return MapOfferRequestResponse(request);
    }

    public async Task<OfferDiscountModificationRequestResponseDto> ApproveRejectOfferRequestAsync(int requestId, ApproveRejectModificationRequestDto dto, int ownerId, CancellationToken ct = default)
    {
        var request = await _uow.OfferDiscountModificationRequests.GetByIdAsync(requestId, ct);
        if (request == null) throw new NotFoundException($"Offer modification request {requestId} not found.");

        if (request.OwnerId != ownerId) throw new ForbiddenException("Only the designated Owner can approve/reject this request.");
        if (request.Status != RequestStatus.Pending) throw new BusinessRuleException("Request is no longer pending.");

        if (dto.Approve)
        {
            request.Status = RequestStatus.Approved;
            await ApplyOfferModificationAsync(request, ct);
        }
        else
        {
            request.Status = RequestStatus.Rejected;
        }

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            request.AdminId,
            dto.Approve ? NotificationType.OfferModificationRequestApproved : NotificationType.OfferModificationRequestRejected,
            NotificationPriority.Normal,
            dto.Approve ? "Offer Request Approved" : "Offer Request Rejected",
            dto.Approve ? "Your offer modification request was approved." : "Your offer modification request was rejected.",
            request.Id, "OfferDiscountModificationRequest", ct);

        return MapOfferRequestResponse(request);
    }

    public async Task<PagedResultDto<OfferDiscountModificationRequestResponseDto>> GetOfferRequestsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.OfferDiscountModificationRequests.GetPagedAsync(page, pageSize, ct: ct);
        return new PagedResultDto<OfferDiscountModificationRequestResponseDto>
        {
            Items = paged.Items.Select(MapOfferRequestResponse).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    private async Task ApplyOfferModificationAsync(OfferDiscountModificationRequest request, CancellationToken ct)
    {
        if (request.RequestType == RequestType.Add)
        {
            // New offer created — Owner must activate explicitly (BR-58)
            var offer = new OfferDiscount
            {
                Title = request.ProposedTitle ?? "New Offer",
                Description = request.ProposedDescription,
                DiscountPercentage = request.ProposedDiscountPercentage ?? 0,
                StartDate = request.ProposedStartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = request.ProposedEndDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
                BranchId = request.ProposedBranchId ?? 0,
                ServiceId = request.ProposedServiceId,
                IsActive = false // BR-58: Owner activates explicitly
            };
            await _uow.OfferDiscounts.AddAsync(offer, ct);
        }
        else if (request.RequestType == RequestType.Update && request.OfferId.HasValue)
        {
            var offer = await _uow.OfferDiscounts.GetByIdAsync(request.OfferId.Value, ct);
            if (offer != null)
            {
                if (request.ProposedTitle != null) offer.Title = request.ProposedTitle;
                if (request.ProposedDescription != null) offer.Description = request.ProposedDescription;
                if (request.ProposedDiscountPercentage.HasValue) offer.DiscountPercentage = request.ProposedDiscountPercentage.Value;
                if (request.ProposedStartDate.HasValue) offer.StartDate = request.ProposedStartDate.Value;
                if (request.ProposedEndDate.HasValue) offer.EndDate = request.ProposedEndDate.Value;
                if (request.ProposedBranchId.HasValue) offer.BranchId = request.ProposedBranchId.Value;
                if (request.ProposedServiceId.HasValue) offer.ServiceId = request.ProposedServiceId.Value;
            }
        }
        else if (request.RequestType == RequestType.Delete && request.OfferId.HasValue)
        {
            var offer = await _uow.OfferDiscounts.GetByIdAsync(request.OfferId.Value, ct);
            if (offer != null) offer.IsActive = false;
        }
    }

    private static OfferDiscountModificationRequestResponseDto MapOfferRequestResponse(OfferDiscountModificationRequest r) => new()
    {
        Id = r.Id,
        AdminId = r.AdminId,
        AdminName = r.Admin?.FullName ?? string.Empty,
        OwnerId = r.OwnerId,
        OwnerName = r.Owner?.FullName ?? string.Empty,
        OfferId = r.OfferId,
        RequestType = r.RequestType,
        Status = r.Status,
        ProposedTitle = r.ProposedTitle,
        ProposedDiscountPercentage = r.ProposedDiscountPercentage,
        ProposedStartDate = r.ProposedStartDate,
        ProposedEndDate = r.ProposedEndDate,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };

    // ─── Branch Modification ─────────────────────────────────

    public async Task<BranchModificationRequestResponseDto> CreateBranchRequestAsync(CreateBranchModificationRequestDto dto, CancellationToken ct = default)
    {
        var admin = await _uow.Admins.GetByIdAsync(dto.AdminId, ct);
        if (admin == null) throw new NotFoundException("Admin not found.");

        var request = new BranchModificationRequest
        {
            AdminId = dto.AdminId,
            OwnerId = dto.OwnerId,
            BranchId = dto.BranchId,
            RequestType = dto.RequestType,
            Status = RequestStatus.Pending,
            ProposedName = dto.ProposedName,
            ProposedLocation = dto.ProposedLocation,
            ProposedPhone = dto.ProposedPhone,
            ProposedWorkingHours = dto.ProposedWorkingHours
        };

        await _uow.BranchModificationRequests.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            dto.OwnerId, NotificationType.BranchModificationRequestSubmitted, NotificationPriority.Normal,
            "Branch Modification Request", "A new branch modification request awaits your review.",
            request.Id, "BranchModificationRequest", ct);

        return MapBranchRequestResponse(request);
    }

    public async Task<BranchModificationRequestResponseDto> ApproveRejectBranchRequestAsync(int requestId, ApproveRejectModificationRequestDto dto, int ownerId, CancellationToken ct = default)
    {
        var request = await _uow.BranchModificationRequests.GetByIdAsync(requestId, ct);
        if (request == null) throw new NotFoundException($"Branch modification request {requestId} not found.");

        if (request.OwnerId != ownerId) throw new ForbiddenException("Only the designated Owner can approve/reject this request.");
        if (request.Status != RequestStatus.Pending) throw new BusinessRuleException("Request is no longer pending.");

        if (dto.Approve)
        {
            request.Status = RequestStatus.Approved;
            await ApplyBranchModificationAsync(request, ct);
        }
        else
        {
            request.Status = RequestStatus.Rejected;
        }

        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            request.AdminId,
            dto.Approve ? NotificationType.BranchModificationRequestApproved : NotificationType.BranchModificationRequestRejected,
            NotificationPriority.Normal,
            dto.Approve ? "Branch Request Approved" : "Branch Request Rejected",
            dto.Approve ? "Your branch modification request was approved." : "Your branch modification request was rejected.",
            request.Id, "BranchModificationRequest", ct);

        return MapBranchRequestResponse(request);
    }

    public async Task<PagedResultDto<BranchModificationRequestResponseDto>> GetBranchRequestsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.BranchModificationRequests.GetPagedAsync(page, pageSize, ct: ct);
        return new PagedResultDto<BranchModificationRequestResponseDto>
        {
            Items = paged.Items.Select(MapBranchRequestResponse).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    private async Task ApplyBranchModificationAsync(BranchModificationRequest request, CancellationToken ct)
    {
        if (request.RequestType == RequestType.Add)
        {
            var branch = new Branch
            {
                Name = request.ProposedName!,
                Location = request.ProposedLocation!,
                Phone = request.ProposedPhone!,
                WorkingHours = request.ProposedWorkingHours,
                IsActive = true
            };
            await _uow.Branches.AddAsync(branch, ct);
        }
        else if (request.RequestType == RequestType.Update && request.BranchId.HasValue)
        {
            var branch = await _uow.Branches.GetByIdAsync(request.BranchId.Value, ct);
            if (branch != null)
            {
                if (request.ProposedName != null) branch.Name = request.ProposedName;
                if (request.ProposedLocation != null) branch.Location = request.ProposedLocation;
                if (request.ProposedPhone != null) branch.Phone = request.ProposedPhone;
                if (request.ProposedWorkingHours != null) branch.WorkingHours = request.ProposedWorkingHours;
            }
        }
        else if (request.RequestType == RequestType.Delete && request.BranchId.HasValue)
        {
            var branch = await _uow.Branches.GetByIdAsync(request.BranchId.Value, ct);
            if (branch != null) branch.IsActive = false;
        }
    }

    private static BranchModificationRequestResponseDto MapBranchRequestResponse(BranchModificationRequest r) => new()
    {
        Id = r.Id,
        AdminId = r.AdminId,
        AdminName = r.Admin?.FullName ?? string.Empty,
        OwnerId = r.OwnerId,
        OwnerName = r.Owner?.FullName ?? string.Empty,
        BranchId = r.BranchId,
        RequestType = r.RequestType,
        Status = r.Status,
        ProposedName = r.ProposedName,
        ProposedLocation = r.ProposedLocation,
        ProposedPhone = r.ProposedPhone,
        ProposedWorkingHours = r.ProposedWorkingHours,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };
}
