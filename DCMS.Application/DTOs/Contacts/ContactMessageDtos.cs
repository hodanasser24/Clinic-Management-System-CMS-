using DCMS.Domain.Enums;

namespace DCMS.Application.DTOs.Contacts;

// ── Requests ─────────────────────────────────────────────────────────────────

/// <summary>
/// Client-submitted contact message request.
/// SenderType/SenderId are set server-side by the service from ICurrentUserService — never by the client.
/// BR-45: Guests (unauthenticated) may only submit Type = General.
/// </summary>
public class CreateContactMessageRequestDto
{
    public string SenderName  { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Subject     { get; set; } = string.Empty;
    public string Body        { get; set; } = string.Empty;
    public ContactMessageType Type { get; set; } = ContactMessageType.General;
}

public class ReplyContactMessageRequestDto
{
    public string ReplyBody { get; set; } = string.Empty;
}

public class UpdateContactMessageStatusRequestDto
{
    public ContactMessageStatus Status { get; set; }
}

// ── Filters ───────────────────────────────────────────────────────────────────

public class ContactMessageFilterRequestDto
{
    public ContactMessageType?   Type     { get; set; }
    public ContactMessageStatus? Status   { get; set; }
    public string?               Search   { get; set; }
    public DateTime?             From     { get; set; }
    public DateTime?             To       { get; set; }
    public int Page     { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// ── Responses ─────────────────────────────────────────────────────────────────

public class ContactMessageResponseDto
{
    public int    Id          { get; set; }
    public string SenderName  { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Subject     { get; set; } = string.Empty;
    public string Body        { get; set; } = string.Empty;

    public SenderType           SenderType   { get; set; }
    public int?                 SenderId     { get; set; }

    public ContactMessageType   Type         { get; set; }
    public string               TypeLabel    { get; set; } = string.Empty;
    public ContactMessageStatus Status       { get; set; }
    public string               StatusLabel  { get; set; } = string.Empty;

    public string?   ReplyBody  { get; set; }
    public DateTime? RepliedAt  { get; set; }
    public string?   RepliedBy  { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ContactMessageSummaryResponseDto
{
    public int    Id         { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Subject    { get; set; } = string.Empty;

    public SenderType           SenderType   { get; set; }
    public ContactMessageType   Type         { get; set; }
    public string               TypeLabel    { get; set; } = string.Empty;
    public ContactMessageStatus Status       { get; set; }
    public string               StatusLabel  { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public bool     HasReply  { get; set; }
}

public class PagedContactMessageResponseDto
{
    public IEnumerable<ContactMessageSummaryResponseDto> Items      { get; set; } = [];
    public int TotalCount  { get; set; }
    public int Page        { get; set; }
    public int PageSize    { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public Dictionary<string, int> CountByType   { get; set; } = [];
    public Dictionary<string, int> CountByStatus { get; set; } = [];
}
