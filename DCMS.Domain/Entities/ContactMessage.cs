using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

/// <summary>
/// SRS §5.1, BR-12, BR-45, BR-55.
/// SenderType/SenderId added to properly identify authenticated vs. guest senders.
/// Routing: General → Admin notification, Complaint/Suggestion → Owner notification.
/// </summary>
public class ContactMessage : BaseEntity
{
    // Sender display info (filled by client; always present)
    public string SenderName  { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Subject     { get; set; } = string.Empty;
    public string Body        { get; set; } = string.Empty;

    // SRS-required sender identity fields
    public SenderType SenderType   { get; set; } = SenderType.Guest;
    public int?       SenderId     { get; set; }         // UserId for Patient senders; null for guests
    public string?    GuestSessionId { get; set; }       // BR-55: captures guest SessionId

    public ContactMessageType   Type   { get; set; } = ContactMessageType.General;
    public ContactMessageStatus Status { get; set; } = ContactMessageStatus.New;  // SRS initial state

    // Reply tracking
    public string?   ReplyBody       { get; set; }
    public DateTime? RepliedAt       { get; set; }
    public int?      RepliedByUserId { get; set; }
    public User?     RepliedByUser   { get; set; }

    // Soft-archive support
    public bool IsArchived { get; set; } = false;
}
