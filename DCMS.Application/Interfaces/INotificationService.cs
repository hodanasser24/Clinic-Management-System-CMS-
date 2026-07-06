using DCMS.Application.DTOs.Notifications;
using DCMS.Application.DTOs.Common;
using DCMS.Domain.Enums;

namespace DCMS.Application.Interfaces;

public interface INotificationService
{
    Task<PagedNotificationResponseDto> GetByUserAsync(int userId, int page, int pageSize, bool unreadOnly = false, CancellationToken ct = default);
    Task MarkAsReadAsync(int notificationId, int userId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(int userId, CancellationToken ct = default);
    Task SendAsync(int userId, NotificationType type, NotificationPriority priority, string title, string message, int? relatedEntityId = null, string? relatedEntityType = null, CancellationToken ct = default);
    Task SendToRoleAsync(UserRole role, NotificationType type, NotificationPriority priority, string title, string message, int? relatedEntityId = null, string? relatedEntityType = null, CancellationToken ct = default);
}
