namespace DCMS.Domain.Enums;

/// <summary>
/// SRS-defined contact message types.
/// BR-12: General → Admin, Complaint/Suggestion → Owner.
/// Previous implementation had 8 values; trimmed to the 3 specified in the SRS.
/// Values are stored as strings in the DB (HasConversion&lt;string&gt;), so trimming is safe.
/// </summary>
public enum ContactMessageType
{
    General    = 0,
    Complaint  = 1,
    Suggestion = 2
}
