namespace DCMS.Domain.Enums;

/// <summary>
/// SRS-aligned status lifecycle for contact messages.
/// Initial state is New (was incorrectly Pending in previous version).
/// </summary>
public enum ContactMessageStatus
{
    New        = 0,   // SRS: initial state — renamed from Pending
    Read       = 1,   // Viewed by staff
    InProgress = 2,   // Being handled
    Replied    = 3,   // Staff replied
    Closed     = 4,   // Fully resolved
    Archived   = 5    // Soft-archived
}
