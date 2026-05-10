using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

public class ContactMessage : BaseEntity
{
    public string SenderName  { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Subject     { get; set; } = string.Empty;
    public string Body        { get; set; } = string.Empty;

    public ContactMessageType   Type   { get; set; } = ContactMessageType.General;
    public ContactMessageStatus Status { get; set; } = ContactMessageStatus.Pending;

    // Reply
    public string? ReplyBody  { get; set; }
    public DateTime? RepliedAt { get; set; }

    // FK to the staff member who replied (nullable – may be replied by admin or owner)
    public int? RepliedByUserId { get; set; }
    public User? RepliedByUser { get; set; }

    // Soft-delete / archive
    public bool IsArchived { get; set; } = false;
}
