using System;
using System.Collections.Generic;
using DCMS.Domain.Enums;

namespace DCMS.Application.DTOs.Contacts;

// ── Requests ────────────────────────────────────────────────────────────────

public class CreateContactMessageRequestDto
{
    public string SenderName   { get; set; } = string.Empty;
    public string SenderEmail  { get; set; } = string.Empty;
    public string Subject      { get; set; } = string.Empty;
    public string Body         { get; set; } = string.Empty;
    public ContactMessageType  Type { get; set; } = ContactMessageType.General;
}

public class ReplyContactMessageRequestDto
{
    public string ReplyBody { get; set; } = string.Empty;
}

public class UpdateContactMessageStatusRequestDto
{
    public ContactMessageStatus Status { get; set; }
}

// ── Filters / Pagination ─────────────────────────────────────────────────────

/// <summary>
/// Query object for GetByType – all fields are optional.
/// Passing no Type returns all messages (admin convenience).
/// </summary>
public class ContactMessageFilterRequestDto
{
    /// <summary>Filter by message type. Null = return all types.</summary>
    public ContactMessageType? Type { get; set; }

    /// <summary>Filter by status (e.g. only Pending). Null = all statuses.</summary>
    public ContactMessageStatus? Status { get; set; }

    /// <summary>Case-insensitive substring match on SenderName or Subject.</summary>
    public string? Search { get; set; }

    /// <summary>Inclusive lower bound for CreatedAt.</summary>
    public DateTime? From { get; set; }

    /// <summary>Inclusive upper bound for CreatedAt.</summary>
    public DateTime? To { get; set; }

    // Pagination
    public int Page     { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// ── Responses ────────────────────────────────────────────────────────────────

public class ContactMessageResponseDto
{
    public int    Id          { get; set; }
    public string SenderName  { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Subject     { get; set; } = string.Empty;
    public string Body        { get; set; } = string.Empty;

    public ContactMessageType   Type      { get; set; }
    public string               TypeLabel { get; set; } = string.Empty;

    public ContactMessageStatus Status      { get; set; }
    public string               StatusLabel { get; set; } = string.Empty;

    public string? ReplyBody    { get; set; }
    public DateTime? RepliedAt  { get; set; }
    public string? RepliedBy    { get; set; }   // Admin/Owner full-name

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ContactMessageSummaryResponseDto
{
    public int    Id          { get; set; }
    public string SenderName  { get; set; } = string.Empty;
    public string Subject     { get; set; } = string.Empty;
    public ContactMessageType   Type      { get; set; }
    public string               TypeLabel { get; set; } = string.Empty;
    public ContactMessageStatus Status      { get; set; }
    public string               StatusLabel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool     HasReply  { get; set; }
}

public class PagedContactMessageResponseDto
{
    public IEnumerable<ContactMessageSummaryResponseDto> Items      { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page       { get; set; }
    public int PageSize   { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    // Aggregates useful for admin dashboards
    public Dictionary<string, int> CountByType   { get; set; } = [];
    public Dictionary<string, int> CountByStatus { get; set; } = [];
}
