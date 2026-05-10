using DCMS.Domain.Common;

namespace DCMS.Domain.Entities;

public class SystemLog : BaseEntity
{
    public int? UserId { get; set; }
    public string? UserRole { get; set; }

    public string ActionType { get; set; } = string.Empty; // e.g. "POST /api/appointment"
    public string EntityType { get; set; } = string.Empty; // e.g. "Appointment"
    public int? EntityId { get; set; }

    public int? HttpStatusCode { get; set; }
    public string? IPAddress { get; set; }

    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON

    public DateTime Date { get; set; } = DateTime.UtcNow;
    public DateTime RetentionDate { get; set; } = DateTime.UtcNow.AddYears(2);

    // Navigation Properties
    public virtual User? User { get; set; }
}
