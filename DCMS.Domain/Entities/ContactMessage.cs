using DCMS.Domain.Common;
using DCMS.Domain.Enums;

namespace DCMS.Domain.Entities;

public class ContactMessage : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public ContactMessageType Type { get; set; }
    public ContactMessageStatus Status { get; set; } = ContactMessageStatus.New;
    
    public SenderType SenderType { get; set; }
    public int? SenderId { get; set; } // Null for Guest

    // Navigation property
    public virtual Patient? Patient { get; set; } 
}
