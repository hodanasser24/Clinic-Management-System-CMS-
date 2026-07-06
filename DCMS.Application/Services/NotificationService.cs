using DCMS.Application.DTOs.Common;
using DCMS.Application.DTOs.Notifications;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;

using AutoMapper;

namespace DCMS.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper     _mapper;

    public NotificationService(IUnitOfWork uow, IMapper mapper)
    {
        _uow    = uow;
        _mapper = mapper;
    }

    // FIXED: predicate passed BEFORE paging — was filtering in-memory after paging (bug)
    public async Task<PagedNotificationResponseDto> GetByUserAsync(int userId, int page, int pageSize, bool unreadOnly = false, CancellationToken ct = default)
    {
        var paged = await _uow.Notifications.GetPagedAsync(page, pageSize, n => n.UserId == userId && (!unreadOnly || !n.IsRead), ct);
        var unreadCount = await _uow.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);

        return new PagedNotificationResponseDto
        {
            Items = _mapper.Map<List<NotificationResponseDto>>(paged.Items),
            TotalCount = paged.TotalCount,
            UnreadCount = unreadCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task MarkAsReadAsync(int notificationId, int userId, CancellationToken ct = default)
    {
        var notification = await _uow.Notifications.GetByIdAsync(notificationId, ct);
        if (notification == null) throw new NotFoundException($"Notification {notificationId} not found.");
        if (notification.UserId != userId) throw new ForbiddenException("Cannot mark another user's notification as read.");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(int userId, CancellationToken ct = default)
    {
        var all = await _uow.Notifications.FindAsync(n => n.UserId == userId && !n.IsRead, ct);
        var now = DateTime.UtcNow;
        foreach (var n in all)
        {
            n.IsRead = true;
            n.ReadAt = now;
        }
        await _uow.SaveChangesAsync(ct);
    }

    public async Task SendAsync(int userId, NotificationType type, NotificationPriority priority, string title, string message, int? relatedEntityId = null, string? relatedEntityType = null, CancellationToken ct = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Priority = priority,
            Title = title,
            Message = message,
            IsRead = false,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType
        };

        await _uow.Notifications.AddAsync(notification, ct);
        await _uow.SaveChangesAsync(ct);
    }

    // BR-24/25: Send to all active users with a given role
    public async Task SendToRoleAsync(UserRole role, NotificationType type, NotificationPriority priority, string title, string message, int? relatedEntityId = null, string? relatedEntityType = null, CancellationToken ct = default)
    {
        IEnumerable<User> users;

        if (role == UserRole.Patient)
        {
            var patients = await _uow.Patients.GetAllAsync(ct);
            users = patients.Cast<User>();
        }
        else if (role == UserRole.Doctor)
        {
            var doctors = await _uow.Doctors.GetAllAsync(ct);
            users = doctors.Cast<User>().Where(u => u.Role == UserRole.Doctor);
        }
        else if (role == UserRole.Owner)
        {
            var doctors = await _uow.Doctors.GetAllAsync(ct);
            users = doctors.Cast<User>().Where(u => u.Role == UserRole.Owner);
        }
        else if (role == UserRole.Admin)
        {
            var admins = await _uow.Admins.GetAllAsync(ct);
            users = admins.Cast<User>();
        }
        else
        {
            return;
        }

        foreach (var user in users.Where(u => u.IsActive))
        {
            var notification = new Notification
            {
                UserId = user.Id,
                Type = type,
                Priority = priority,
                Title = title,
                Message = message,
                IsRead = false,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType
            };
            await _uow.Notifications.AddAsync(notification, ct);
        }

        await _uow.SaveChangesAsync(ct);
    }

}
