using AutoMapper;
using DCMS.Application.DTOs.Contacts;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Common;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;

namespace DCMS.Application.Services;

public class ContactMessageService : IContactMessageService
{
    private readonly IUnitOfWork          _uow;
    private readonly INotificationService _notifications;
    private readonly ICurrentUserService  _currentUser;
    private readonly IMapper              _mapper;

    public ContactMessageService(
        IUnitOfWork          uow,
        INotificationService notifications,
        ICurrentUserService  currentUser,
        IMapper              mapper)
    {
        _uow           = uow;
        _notifications = notifications;
        _currentUser   = currentUser;
        _mapper        = mapper;
    }

    // ── Public ───────────────────────────────────────────────────────────────

    public async Task<ContactMessageResponseDto> CreateAsync(
        CreateContactMessageRequestDto request,
        CancellationToken ct = default)
    {
        var message = _mapper.Map<ContactMessage>(request);
        
        await _uow.ContactMessages.AddAsync(message, ct);
        await _uow.SaveChangesAsync(ct);

        // Notify admins about new contact message
        await _notifications.SendToRoleAsync(
            UserRole.Admin,
            NotificationType.NewContactMessage,
            NotificationPriority.Normal,
            "New Contact Message",
            $"New contact message from {message.SenderName}: {message.Subject}",
            relatedEntityId: message.Id,
            relatedEntityType: "ContactMessage",
            ct: ct);

        return _mapper.Map<ContactMessageResponseDto>(message);
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public Task<PagedContactMessageResponseDto> GetAllAsync(
        ContactMessageFilterRequestDto filter,
        CancellationToken ct = default)
    {
        // GetAll is GetByType without a type constraint
        filter.Type = null;
        return GetByTypeAsync(filter, ct);
    }

    public async Task<PagedContactMessageResponseDto> GetByTypeAsync(
        ContactMessageFilterRequestDto filter,
        CancellationToken ct = default)
    {
        var allMessages = await _uow.ContactMessages.FindAsync(m => 
            !m.IsArchived &&
            (!filter.Type.HasValue || m.Type == filter.Type.Value) &&
            (!filter.Status.HasValue || m.Status == filter.Status.Value) &&
            (!filter.From.HasValue || m.CreatedAt >= filter.From.Value) &&
            (!filter.To.HasValue || m.CreatedAt <= filter.To.Value), ct);

        var query = allMessages.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(m =>
                m.SenderName.ToLower().Contains(term) ||
                m.Subject.ToLower().Contains(term)    ||
                m.SenderEmail.ToLower().Contains(term));
        }

        var filteredList = query.ToList();
        var totalCount = filteredList.Count;

        var countByType = filteredList
            .GroupBy(m => m.Type)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var countByStatus = filteredList
            .GroupBy(m => m.Status)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = filteredList
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedContactMessageResponseDto
        {
            Items         = _mapper.Map<IEnumerable<ContactMessageSummaryResponseDto>>(items),
            TotalCount    = totalCount,
            Page          = page,
            PageSize      = pageSize,
            CountByType   = countByType,
            CountByStatus = countByStatus
        };
    }

    public async Task<ContactMessageResponseDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var message = await FindOrThrowAsync(id, ct);

        // Auto-mark as Read when first viewed by staff
        if (message.Status == ContactMessageStatus.Pending)
        {
            message.Status = ContactMessageStatus.Read;
            _uow.ContactMessages.Update(message);
            await _uow.SaveChangesAsync(ct);
        }

        return _mapper.Map<ContactMessageResponseDto>(message);
    }

    // ── Mutations ─────────────────────────────────────────────────────────────

    public async Task<ContactMessageResponseDto> ReplyAsync(
        int id,
        ReplyContactMessageRequestDto request,
        int repliedByUserId,
        CancellationToken ct = default)
    {
        var message = await FindOrThrowAsync(id, ct);

        if (message.Status == ContactMessageStatus.Closed)
            throw new InvalidOperationException("Cannot reply to a closed message.");

        message.ReplyBody       = request.ReplyBody.Trim();
        message.RepliedAt       = DateTime.UtcNow;
        message.RepliedByUserId = repliedByUserId;
        message.Status          = ContactMessageStatus.Replied;
        message.UpdatedAt       = DateTime.UtcNow;

        _uow.ContactMessages.Update(message);
        await _uow.SaveChangesAsync(ct);

        // TODO: Send email reply to message.SenderEmail via IEmailService

        return _mapper.Map<ContactMessageResponseDto>(message);
    }

    public async Task<ContactMessageResponseDto> UpdateStatusAsync(
        int id,
        UpdateContactMessageStatusRequestDto request,
        CancellationToken ct = default)
    {
        var message = await FindOrThrowAsync(id, ct);

        message.Status    = request.Status;
        message.UpdatedAt = DateTime.UtcNow;

        _uow.ContactMessages.Update(message);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ContactMessageResponseDto>(message);
    }

    public async Task ArchiveAsync(int id, CancellationToken ct = default)
    {
        var message = await FindOrThrowAsync(id, ct);
        message.IsArchived = true;
        message.UpdatedAt  = DateTime.UtcNow;
        
        _uow.ContactMessages.Update(message);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var message = await FindOrThrowAsync(id, ct);
        _uow.ContactMessages.Remove(message);
        await _uow.SaveChangesAsync(ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<ContactMessage> FindOrThrowAsync(int id, CancellationToken ct)
    {
        var msg = await _uow.ContactMessages.GetByIdAsync(id, ct);
        if (msg == null || msg.IsArchived) throw new NotFoundException($"{nameof(ContactMessage)} with ID {id} not found.");

        if (msg.RepliedByUserId.HasValue)
        {
            var admin = await _uow.Admins.GetByIdAsync(msg.RepliedByUserId.Value, ct);
            if (admin != null) 
            {
                msg.RepliedByUser = admin;
            }
            else 
            {
                var doc = await _uow.Doctors.GetByIdAsync(msg.RepliedByUserId.Value, ct);
                if (doc != null) msg.RepliedByUser = doc;
            }
        }

        return msg;
    }
}
