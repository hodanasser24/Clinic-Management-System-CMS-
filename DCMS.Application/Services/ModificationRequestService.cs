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
    private readonly IUnitOfWork          _uow;
    private readonly INotificationService _notificationService;

    public ModificationRequestService(
        IUnitOfWork uow, INotificationService notificationService)
    {
        _uow                 = uow;
        _notificationService = notificationService;
    }

    // ── SERVICE MODIFICATION ──────────────────────────────────────────────────

    public async Task<ServiceModificationRequestResponseDto> CreateServiceRequestAsync(
        CreateServiceModificationRequestDto dto, CancellationToken ct = default)
    {
        if (await _uow.Admins.GetByIdAsync(dto.AdminId, ct) == null)
            throw new NotFoundException("Admin not found.");

        var request = new ServiceModificationRequest
        {
            AdminId              = dto.AdminId,
            OwnerId              = dto.OwnerId,
            ServiceId            = dto.ServiceId,
            RequestType          = dto.RequestType,
            Status               = RequestStatus.Pending,
            ProposedName         = dto.ProposedName,
            ProposedPrice        = dto.ProposedPrice,
            ProposedDescription  = dto.ProposedDescription,
            ProposedEstimatedDurationMinutes = dto.ProposedEstimatedDurationMinutes,
            Reason               = null
        };

        await _uow.ServiceModificationRequests.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            dto.OwnerId, NotificationType.ServiceModificationRequestSubmitted,
            NotificationPriority.Normal,
            "Service Modification Request",
            "A new service modification request awaits your review.",
            request.Id, "ServiceModificationRequest", ct);

        return await MapServiceResponseAsync(request, ct);
    }

    public async Task<ServiceModificationRequestResponseDto> ApproveRejectServiceRequestAsync(
        int requestId, ApproveRejectModificationRequestDto dto, int ownerId,
        CancellationToken ct = default)
    {
        var request = await _uow.ServiceModificationRequests.GetByIdAsync(requestId, ct)
            ?? throw new NotFoundException($"Service modification request {requestId} not found.");

        if (request.OwnerId != ownerId)
            throw new ForbiddenException("Only the designated Owner can decide on this request.");
        if (request.Status != RequestStatus.Pending)
            throw new BusinessRuleException("Request is no longer pending.");

        // Wrap in transaction: status change + entity mutation must be atomic
        await _uow.BeginTransactionAsync(ct);
        try
        {
            if (dto.Approve)
            {
                request.Status      = RequestStatus.Approved;
                request.ApprovalDate = DateTime.UtcNow;
                await ApplyServiceChangeAsync(request, ct);
            }
            else
            {
                request.Status          = RequestStatus.Rejected;
                request.RejectionReason = dto.RejectionReason;
            }

            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }

        var notifType = dto.Approve
            ? NotificationType.ServiceModificationRequestApproved
            : NotificationType.ServiceModificationRequestRejected;

        await _notificationService.SendAsync(
            request.AdminId, notifType, NotificationPriority.Normal,
            dto.Approve ? "Request Approved" : "Request Rejected",
            dto.Approve
                ? "Your service modification request was approved."
                : $"Your service modification request was rejected. {dto.RejectionReason}",
            request.Id, "ServiceModificationRequest", ct);

        return await MapServiceResponseAsync(request, ct);
    }

    public async Task<PagedResultDto<ServiceModificationRequestResponseDto>> GetServiceRequestsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.ServiceModificationRequests.GetPagedAsync(page, pageSize, ct: ct);
        var items = new List<ServiceModificationRequestResponseDto>();
        foreach (var r in paged.Items) items.Add(await MapServiceResponseAsync(r, ct));
        return new PagedResultDto<ServiceModificationRequestResponseDto>
        {
            Items = items, TotalCount = paged.TotalCount, Page = paged.Page, PageSize = paged.PageSize
        };
    }

    private async Task ApplyServiceChangeAsync(ServiceModificationRequest r, CancellationToken ct)
    {
        if (r.RequestType == RequestType.Add)
        {
            await _uow.Services.AddAsync(new Service
            {
                Name        = r.ProposedName!,
                Description = r.ProposedDescription,
                Price       = r.ProposedPrice ?? 0,
                EstimatedDurationMinutes = r.ProposedEstimatedDurationMinutes ?? 30,
                IsActive    = true
            }, ct);
        }
        else if (r.RequestType == RequestType.Update && r.ServiceId.HasValue)
        {
            var svc = await _uow.Services.GetByIdAsync(r.ServiceId.Value, ct);
            if (svc == null) return;
            if (r.ProposedName != null)        svc.Name        = r.ProposedName;
            if (r.ProposedPrice.HasValue)      svc.Price       = r.ProposedPrice.Value;
            if (r.ProposedDescription != null) svc.Description = r.ProposedDescription;
            if (r.ProposedEstimatedDurationMinutes.HasValue)
                svc.EstimatedDurationMinutes = r.ProposedEstimatedDurationMinutes.Value;
        }
        else if (r.RequestType == RequestType.Delete && r.ServiceId.HasValue)
        {
            var svc = await _uow.Services.GetByIdAsync(r.ServiceId.Value, ct);
            if (svc != null) svc.IsActive = false;
        }
    }

    private async Task<ServiceModificationRequestResponseDto> MapServiceResponseAsync(
        ServiceModificationRequest r, CancellationToken ct)
    {
        var admin = await _uow.Admins.GetByIdAsync(r.AdminId, ct);
        var owner = await _uow.Doctors.GetByIdAsync(r.OwnerId, ct);
        return new ServiceModificationRequestResponseDto
        {
            Id                              = r.Id,
            AdminId                         = r.AdminId,
            AdminName                       = admin?.FullName ?? string.Empty,
            OwnerId                         = r.OwnerId,
            OwnerName                       = owner?.FullName ?? string.Empty,
            ServiceId                       = r.ServiceId,
            RequestType                     = r.RequestType,
            Status                          = r.Status,
            ProposedName                    = r.ProposedName,
            ProposedPrice                   = r.ProposedPrice,
            ProposedDescription             = r.ProposedDescription,
            ProposedEstimatedDurationMinutes = r.ProposedEstimatedDurationMinutes,
            ApprovalDate                    = r.ApprovalDate,
            CreatedAt                       = r.CreatedAt,
            UpdatedAt                       = r.UpdatedAt
        };
    }

    // ── FAQ MODIFICATION ──────────────────────────────────────────────────────

    public async Task<FAQModificationRequestResponseDto> CreateFAQRequestAsync(
        CreateFAQModificationRequestDto dto, CancellationToken ct = default)
    {
        if (await _uow.Admins.GetByIdAsync(dto.AdminId, ct) == null)
            throw new NotFoundException("Admin not found.");

        var request = new FAQModificationRequest
        {
            AdminId          = dto.AdminId,
            OwnerId          = dto.OwnerId,
            FAQId            = dto.FAQId,
            RequestType      = dto.RequestType,
            Status           = RequestStatus.Pending,
            ProposedQuestion = dto.ProposedQuestion,
            ProposedAnswer   = dto.ProposedAnswer
        };

        await _uow.FAQModificationRequests.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            dto.OwnerId, NotificationType.FAQModificationRequestSubmitted,
            NotificationPriority.Normal, "FAQ Modification Request",
            "A new FAQ modification request awaits your review.",
            request.Id, "FAQModificationRequest", ct);

        return await MapFAQResponseAsync(request, ct);
    }

    public async Task<FAQModificationRequestResponseDto> ApproveRejectFAQRequestAsync(
        int requestId, ApproveRejectModificationRequestDto dto, int ownerId,
        CancellationToken ct = default)
    {
        var request = await _uow.FAQModificationRequests.GetByIdAsync(requestId, ct)
            ?? throw new NotFoundException($"FAQ modification request {requestId} not found.");

        if (request.OwnerId != ownerId)
            throw new ForbiddenException("Only the designated Owner can decide on this request.");
        if (request.Status != RequestStatus.Pending)
            throw new BusinessRuleException("Request is no longer pending.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            if (dto.Approve)
            {
                request.Status = RequestStatus.Approved;
                await ApplyFAQChangeAsync(request, ct);
            }
            else
            {
                request.Status          = RequestStatus.Rejected;
                request.RejectionReason = dto.RejectionReason;
            }
            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }

        return await MapFAQResponseAsync(request, ct);
    }

    public async Task<PagedResultDto<FAQModificationRequestResponseDto>> GetFAQRequestsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.FAQModificationRequests.GetPagedAsync(page, pageSize, ct: ct);
        var items = new List<FAQModificationRequestResponseDto>();
        foreach (var r in paged.Items) items.Add(await MapFAQResponseAsync(r, ct));
        return new PagedResultDto<FAQModificationRequestResponseDto>
        {
            Items = items, TotalCount = paged.TotalCount, Page = paged.Page, PageSize = paged.PageSize
        };
    }

    private async Task ApplyFAQChangeAsync(FAQModificationRequest r, CancellationToken ct)
    {
        if (r.RequestType == RequestType.Add)
        {
            await _uow.FAQs.AddAsync(new FAQ
            {
                Question     = r.ProposedQuestion!,
                Answer       = r.ProposedAnswer!,
                IsActive     = true,
                DisplayOrder = 0
            }, ct);
        }
        else if (r.RequestType == RequestType.Update && r.FAQId.HasValue)
        {
            var faq = await _uow.FAQs.GetByIdAsync(r.FAQId.Value, ct);
            if (faq == null) return;
            if (r.ProposedQuestion != null) faq.Question = r.ProposedQuestion;
            if (r.ProposedAnswer   != null) faq.Answer   = r.ProposedAnswer;
        }
        else if (r.RequestType == RequestType.Delete && r.FAQId.HasValue)
        {
            var faq = await _uow.FAQs.GetByIdAsync(r.FAQId.Value, ct);
            if (faq != null) faq.IsActive = false;
        }
    }

    private async Task<FAQModificationRequestResponseDto> MapFAQResponseAsync(
        FAQModificationRequest r, CancellationToken ct)
    {
        var admin = await _uow.Admins.GetByIdAsync(r.AdminId, ct);
        var owner = await _uow.Doctors.GetByIdAsync(r.OwnerId, ct);
        return new FAQModificationRequestResponseDto
        {
            Id               = r.Id,
            AdminId          = r.AdminId,
            AdminName        = admin?.FullName ?? string.Empty,
            OwnerId          = r.OwnerId,
            OwnerName        = owner?.FullName ?? string.Empty,
            FAQId            = r.FAQId,
            RequestType      = r.RequestType,
            Status           = r.Status,
            ProposedQuestion = r.ProposedQuestion,
            ProposedAnswer   = r.ProposedAnswer,
            CreatedAt        = r.CreatedAt,
            UpdatedAt        = r.UpdatedAt
        };
    }

    // ── OFFER MODIFICATION ────────────────────────────────────────────────────

    public async Task<OfferDiscountModificationRequestResponseDto> CreateOfferRequestAsync(
        CreateOfferDiscountModificationRequestDto dto, CancellationToken ct = default)
    {
        if (await _uow.Admins.GetByIdAsync(dto.AdminId, ct) == null)
            throw new NotFoundException("Admin not found.");

        var request = new OfferDiscountModificationRequest
        {
            AdminId                    = dto.AdminId,
            OwnerId                    = dto.OwnerId,
            OfferId                    = dto.OfferId,
            RequestType                = dto.RequestType,
            Status                     = RequestStatus.Pending,
            ProposedTitle              = dto.ProposedTitle,
            ProposedDescription        = dto.ProposedDescription,
            ProposedDiscountPercentage = dto.ProposedDiscountPercentage,
            ProposedStartDate          = dto.ProposedStartDate,
            ProposedEndDate            = dto.ProposedEndDate,
            ProposedBranchId           = dto.ProposedBranchId,
            ProposedServiceId          = dto.ProposedServiceId
        };

        await _uow.OfferDiscountModificationRequests.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            dto.OwnerId, NotificationType.OfferModificationRequestSubmitted,
            NotificationPriority.Normal, "Offer Modification Request",
            "A new offer modification request awaits your review.",
            request.Id, "OfferDiscountModificationRequest", ct);

        return MapOfferResponse(request);
    }

    public async Task<OfferDiscountModificationRequestResponseDto> ApproveRejectOfferRequestAsync(
        int requestId, ApproveRejectModificationRequestDto dto, int ownerId,
        CancellationToken ct = default)
    {
        var request = await _uow.OfferDiscountModificationRequests.GetByIdAsync(requestId, ct)
            ?? throw new NotFoundException($"Offer modification request {requestId} not found.");

        if (request.OwnerId != ownerId)
            throw new ForbiddenException("Only the designated Owner can decide on this request.");
        if (request.Status != RequestStatus.Pending)
            throw new BusinessRuleException("Request is no longer pending.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            if (dto.Approve)
            {
                request.Status = RequestStatus.Approved;
                await ApplyOfferChangeAsync(request, ct);
            }
            else
            {
                request.Status          = RequestStatus.Rejected;
                request.RejectionReason = dto.RejectionReason;
            }
            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }

        var notifType = dto.Approve
            ? NotificationType.OfferModificationRequestApproved
            : NotificationType.OfferModificationRequestRejected;

        await _notificationService.SendAsync(
            request.AdminId, notifType, NotificationPriority.Normal,
            dto.Approve ? "Offer Request Approved" : "Offer Request Rejected",
            dto.Approve
                ? "Your offer modification request was approved."
                : $"Your offer modification request was rejected. {dto.RejectionReason}",
            request.Id, "OfferDiscountModificationRequest", ct);

        return MapOfferResponse(request);
    }

    public async Task<PagedResultDto<OfferDiscountModificationRequestResponseDto>> GetOfferRequestsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.OfferDiscountModificationRequests.GetPagedAsync(page, pageSize, ct: ct);
        return new PagedResultDto<OfferDiscountModificationRequestResponseDto>
        {
            Items      = paged.Items.Select(MapOfferResponse).ToList(),
            TotalCount = paged.TotalCount,
            Page       = paged.Page,
            PageSize   = paged.PageSize
        };
    }

    private async Task ApplyOfferChangeAsync(OfferDiscountModificationRequest r, CancellationToken ct)
    {
        if (r.RequestType == RequestType.Add)
        {
            // BR-58: new offer starts inactive — Owner must explicitly activate
            await _uow.OfferDiscounts.AddAsync(new OfferDiscount
            {
                Title              = r.ProposedTitle ?? "New Offer",
                Description        = r.ProposedDescription,
                DiscountPercentage = r.ProposedDiscountPercentage ?? 0,
                StartDate          = r.ProposedStartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate            = r.ProposedEndDate   ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
                BranchId           = r.ProposedBranchId  ?? 0,
                ServiceId          = r.ProposedServiceId,
                IsActive           = false
            }, ct);
        }
        else if (r.RequestType == RequestType.Update && r.OfferId.HasValue)
        {
            var offer = await _uow.OfferDiscounts.GetByIdAsync(r.OfferId.Value, ct);
            if (offer == null) return;
            if (r.ProposedTitle              != null) offer.Title              = r.ProposedTitle;
            if (r.ProposedDescription        != null) offer.Description        = r.ProposedDescription;
            if (r.ProposedDiscountPercentage.HasValue) offer.DiscountPercentage = r.ProposedDiscountPercentage.Value;
            if (r.ProposedStartDate.HasValue)          offer.StartDate          = r.ProposedStartDate.Value;
            if (r.ProposedEndDate.HasValue)            offer.EndDate            = r.ProposedEndDate.Value;
            if (r.ProposedBranchId.HasValue)           offer.BranchId           = r.ProposedBranchId.Value;
            if (r.ProposedServiceId.HasValue)          offer.ServiceId          = r.ProposedServiceId.Value;
        }
        else if (r.RequestType == RequestType.Delete && r.OfferId.HasValue)
        {
            var offer = await _uow.OfferDiscounts.GetByIdAsync(r.OfferId.Value, ct);
            if (offer != null) offer.IsActive = false;
        }
    }

    private static OfferDiscountModificationRequestResponseDto MapOfferResponse(
        OfferDiscountModificationRequest r) => new()
    {
        Id                         = r.Id,
        AdminId                    = r.AdminId,
        AdminName                  = r.Admin?.FullName  ?? string.Empty,
        OwnerId                    = r.OwnerId,
        OwnerName                  = r.Owner?.FullName  ?? string.Empty,
        OfferId                    = r.OfferId,
        RequestType                = r.RequestType,
        Status                     = r.Status,
        ProposedTitle              = r.ProposedTitle,
        ProposedDiscountPercentage = r.ProposedDiscountPercentage,
        ProposedStartDate          = r.ProposedStartDate,
        ProposedEndDate            = r.ProposedEndDate,
        CreatedAt                  = r.CreatedAt,
        UpdatedAt                  = r.UpdatedAt
    };

    // ── BRANCH MODIFICATION ───────────────────────────────────────────────────

    public async Task<BranchModificationRequestResponseDto> CreateBranchRequestAsync(
        CreateBranchModificationRequestDto dto, CancellationToken ct = default)
    {
        if (await _uow.Admins.GetByIdAsync(dto.AdminId, ct) == null)
            throw new NotFoundException("Admin not found.");

        var request = new BranchModificationRequest
        {
            AdminId              = dto.AdminId,
            OwnerId              = dto.OwnerId,
            BranchId             = dto.BranchId,
            RequestType          = dto.RequestType,
            Status               = RequestStatus.Pending,
            ProposedName         = dto.ProposedName,
            ProposedLocation     = dto.ProposedLocation,
            ProposedPhone        = dto.ProposedPhone,
            ProposedWorkingHours = dto.ProposedWorkingHours
        };

        await _uow.BranchModificationRequests.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            dto.OwnerId, NotificationType.BranchModificationRequestSubmitted,
            NotificationPriority.Normal, "Branch Modification Request",
            "A new branch modification request awaits your review.",
            request.Id, "BranchModificationRequest", ct);

        return MapBranchResponse(request);
    }

    public async Task<BranchModificationRequestResponseDto> ApproveRejectBranchRequestAsync(
        int requestId, ApproveRejectModificationRequestDto dto, int ownerId,
        CancellationToken ct = default)
    {
        var request = await _uow.BranchModificationRequests.GetByIdAsync(requestId, ct)
            ?? throw new NotFoundException($"Branch modification request {requestId} not found.");

        if (request.OwnerId != ownerId)
            throw new ForbiddenException("Only the designated Owner can decide on this request.");
        if (request.Status != RequestStatus.Pending)
            throw new BusinessRuleException("Request is no longer pending.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            if (dto.Approve)
            {
                request.Status = RequestStatus.Approved;
                await ApplyBranchChangeAsync(request, ct);
            }
            else
            {
                request.Status          = RequestStatus.Rejected;
                request.RejectionReason = dto.RejectionReason;
            }
            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }

        var notifType = dto.Approve
            ? NotificationType.BranchModificationRequestApproved
            : NotificationType.BranchModificationRequestRejected;

        await _notificationService.SendAsync(
            request.AdminId, notifType, NotificationPriority.Normal,
            dto.Approve ? "Branch Request Approved" : "Branch Request Rejected",
            dto.Approve
                ? "Your branch modification request was approved."
                : $"Your branch modification request was rejected. {dto.RejectionReason}",
            request.Id, "BranchModificationRequest", ct);

        return MapBranchResponse(request);
    }

    public async Task<PagedResultDto<BranchModificationRequestResponseDto>> GetBranchRequestsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.BranchModificationRequests.GetPagedAsync(page, pageSize, ct: ct);
        return new PagedResultDto<BranchModificationRequestResponseDto>
        {
            Items      = paged.Items.Select(MapBranchResponse).ToList(),
            TotalCount = paged.TotalCount,
            Page       = paged.Page,
            PageSize   = paged.PageSize
        };
    }

    private async Task ApplyBranchChangeAsync(BranchModificationRequest r, CancellationToken ct)
    {
        if (r.RequestType == RequestType.Add)
        {
            await _uow.Branches.AddAsync(new Branch
            {
                Name         = r.ProposedName!,
                Location     = r.ProposedLocation!,
                Phone        = r.ProposedPhone!,
                WorkingHours = r.ProposedWorkingHours,
                IsActive     = true
            }, ct);
        }
        else if (r.RequestType == RequestType.Update && r.BranchId.HasValue)
        {
            var branch = await _uow.Branches.GetByIdAsync(r.BranchId.Value, ct);
            if (branch == null) return;
            if (r.ProposedName         != null) branch.Name         = r.ProposedName;
            if (r.ProposedLocation     != null) branch.Location     = r.ProposedLocation;
            if (r.ProposedPhone        != null) branch.Phone        = r.ProposedPhone;
            if (r.ProposedWorkingHours != null) branch.WorkingHours = r.ProposedWorkingHours;
        }
        else if (r.RequestType == RequestType.Delete && r.BranchId.HasValue)
        {
            var branch = await _uow.Branches.GetByIdAsync(r.BranchId.Value, ct);
            if (branch != null) branch.IsActive = false;
        }
    }

    private static BranchModificationRequestResponseDto MapBranchResponse(
        BranchModificationRequest r) => new()
    {
        Id                   = r.Id,
        AdminId              = r.AdminId,
        AdminName            = r.Admin?.FullName  ?? string.Empty,
        OwnerId              = r.OwnerId,
        OwnerName            = r.Owner?.FullName  ?? string.Empty,
        BranchId             = r.BranchId,
        RequestType          = r.RequestType,
        Status               = r.Status,
        ProposedName         = r.ProposedName,
        ProposedLocation     = r.ProposedLocation,
        ProposedPhone        = r.ProposedPhone,
        ProposedWorkingHours = r.ProposedWorkingHours,
        CreatedAt            = r.CreatedAt,
        UpdatedAt            = r.UpdatedAt
    };
}
