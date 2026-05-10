using DCMS.Domain.Enums;

namespace DCMS.Application.DTOs.Notifications;

public class NotificationResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MarkNotificationReadRequestDto
{
    public int NotificationId { get; set; }
}

public class PagedNotificationResponseDto
{
    public IEnumerable<NotificationResponseDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
