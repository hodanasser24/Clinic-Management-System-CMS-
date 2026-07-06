// File: DCMS.Domain/Entities/Notification.cs
using System;
using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

public class Notification : BaseEntity
{
    public int UserId { get; set; }

    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
}
