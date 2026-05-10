namespace DCMS.Application.DTOs.SystemLogs;

public class SystemLogResponseDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? UserRole { get; set; }
    public string ActionType { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public int? EntityId { get; set; }
    public int? HttpStatusCode { get; set; }
    public string? IPAddress { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Date { get; set; }
    public DateTime RetentionDate { get; set; }
}

public class SystemLogFilterDto
{
    public string? EntityType { get; set; }
    public string? ActionType { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? UserId { get; set; }
}
