using AutoMapper;
using DCMS.Application.DTOs.Contacts;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DCMS.Application.Services;

public class ContactMessageService : IContactMessageService
{
    private readonly IUnitOfWork           _uow;
    private readonly INotificationService  _notifications;
    private readonly ICurrentUserService   _currentUser;
    private readonly IHttpContextAccessor  _httpContextAccessor;
    private readonly IMapper               _mapper;

    public ContactMessageService(
        IUnitOfWork          uow,
        INotificationService notifications,
        ICurrentUserService  currentUser,
        IHttpContextAccessor httpContextAccessor,
        IMapper              mapper)
    {
        _uow                 = uow;
        _notifications       = notifications;
        _currentUser         = currentUser;
        _httpContextAccessor = httpContextAccessor;
        _mapper              = mapper;
    }

    // ── Public ───────────────────────────────────────────────────────────────

    public async Task<ContactMessageResponseDto> CreateAsync(
        CreateContactMessageRequestDto request,
        CancellationToken ct = default)
    {
        // BR-45: Unauthenticated (Guest) senders may only submit General messages
        if (!_currentUser.IsAuthenticated && request.Type != ContactMessageType.General)
            throw new BusinessRuleException(
                "Guests may only submit General contact messages. " +
                "Please log in to submit a Complaint or Suggestion.");

        var message = _mapper.Map<ContactMessage>(request);

        // Set server-side sender identity
        if (_currentUser.IsAuthenticated && _currentUser.UserId.HasValue)
        {
            message.SenderType = SenderType.Patient;
            message.SenderId   = _currentUser.UserId;
        }
        else
        {
            message.SenderType = SenderType.Guest;
            message.SenderId   = null;

            // BR-55: capture guest SessionId for identity tracking
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx?.Items.TryGetValue("GuestSessionId", out var sid) == true)
                message.GuestSessionId = sid?.ToString();
        }

        await _uow.ContactMessages.AddAsync(message, ct);
        await _uow.SaveChangesAsync(ct);

        // BR-12: Route notifications based on message type
        // General → Admin | Complaint / Suggestion → Owner
        if (request.Type == ContactMessageType.General)
        {
            await _notifications.SendToRoleAsync(
                UserRole.Admin, NotificationType.NewContactMessage, NotificationPriority.Normal,
                "New General Message",
                $"New general message from {message.SenderName}: {message.Subject}",
                message.Id, "ContactMessage", ct);
        }
        else
        {
            // Complaint or Suggestion → Owner
            await _notifications.SendToRoleAsync(
                UserRole.Owner, NotificationType.NewContactMessage, NotificationPriority.High,
                $"New {request.Type} Message",
                $"New {request.Type.ToString().ToLower()} from {message.SenderName}: {message.Subject}",
                message.Id, "ContactMessage", ct);
        }

        return _mapper.Map<ContactMessageResponseDto>(message);
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public Task<PagedContactMessageResponseDto> GetAllAsync(
        ContactMessageFilterRequestDto filter, CancellationToken ct = default)
    {
        filter.Type = null;   // GetAll = no type filter
        return GetByTypeAsync(filter, ct);
    }

    public async Task<PagedContactMessageResponseDto> GetByTypeAsync(
        ContactMessageFilterRequestDto filter, CancellationToken ct = default)
    {
        var all = await _uow.ContactMessages.FindAsync(m =>
            (!filter.Type.HasValue   || m.Type   == filter.Type.Value)   &&
            (!filter.Status.HasValue || m.Status == filter.Status.Value) &&
            (!filter.From.HasValue   || m.CreatedAt >= filter.From.Value) &&
            (!filter.To.HasValue     || m.CreatedAt <= filter.To.Value),
            ct);

        var query = all.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(m =>
                m.SenderName.ToLower().Contains(term)  ||
                m.Subject.ToLower().Contains(term)     ||
                m.SenderEmail.ToLower().Contains(term));
        }

        var list       = query.OrderByDescending(m => m.CreatedAt).ToList();
        var totalCount = list.Count;
        var page       = Math.Max(1, filter.Page);
        var pageSize   = Math.Clamp(filter.PageSize, 1, 100);

        var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedContactMessageResponseDto
        {
            Items         = _mapper.Map<IEnumerable<ContactMessageSummaryResponseDto>>(items),
            TotalCount    = totalCount,
            Page          = page,
            PageSize      = pageSize,
            CountByType   = list.GroupBy(m => m.Type).ToDictionary(g => g.Key.ToString(), g => g.Count()),
            CountByStatus = list.GroupBy(m => m.Status).ToDictionary(g => g.Key.ToString(), g => g.Count())
        };
    }

    public async Task<ContactMessageResponseDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var message = await FindOrThrowAsync(id, ct);

        // Auto-mark as Read on first staff view
        if (message.Status == ContactMessageStatus.New)
        {
            message.Status = ContactMessageStatus.Read;
            _uow.ContactMessages.Update(message);
            await _uow.SaveChangesAsync(ct);
        }

        return _mapper.Map<ContactMessageResponseDto>(message);
    }

    // ── Mutations ─────────────────────────────────────────────────────────────

    public async Task<ContactMessageResponseDto> ReplyAsync(
        int id, ReplyContactMessageRequestDto request, int repliedByUserId, CancellationToken ct = default)
    {
        var message = await FindOrThrowAsync(id, ct);
        if (message.Status == ContactMessageStatus.Closed)
            throw new BusinessRuleException("Cannot reply to a closed message.");

        message.ReplyBody       = request.ReplyBody.Trim();
        message.RepliedAt       = DateTime.UtcNow;
        message.RepliedByUserId = repliedByUserId;
        message.Status          = ContactMessageStatus.Replied;

        _uow.ContactMessages.Update(message);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<ContactMessageResponseDto>(message);
    }

    public async Task<ContactMessageResponseDto> UpdateStatusAsync(
        int id, UpdateContactMessageStatusRequestDto request, CancellationToken ct = default)
    {
        var message = await FindOrThrowAsync(id, ct);
        message.Status = request.Status;

        _uow.ContactMessages.Update(message);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<ContactMessageResponseDto>(message);
    }

    public async Task ArchiveAsync(int id, CancellationToken ct = default)
    {
        var message = await FindOrThrowAsync(id, ct);
        message.IsArchived = true;
        _uow.ContactMessages.Update(message);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        // Bypass global query filter to allow hard-delete of archived messages
        var messages = await _uow.ContactMessages.FindAsync(m => m.Id == id, ct);
        var message  = messages.FirstOrDefault()
            ?? throw new NotFoundException($"ContactMessage {id} not found.");

        _uow.ContactMessages.Remove(message);
        await _uow.SaveChangesAsync(ct);
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private async Task<ContactMessage> FindOrThrowAsync(int id, CancellationToken ct)
    {
        var msg = await _uow.ContactMessages.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"ContactMessage {id} not found.");

        // Eagerly load replied-by user for DTO mapping
        if (msg.RepliedByUserId.HasValue && msg.RepliedByUser == null)
        {
            User? replier = await _uow.Admins.GetByIdAsync(msg.RepliedByUserId.Value, ct);
            replier ??= await _uow.Doctors.GetByIdAsync(msg.RepliedByUserId.Value, ct);
            msg.RepliedByUser = replier;
        }

        return msg;
    }
}
