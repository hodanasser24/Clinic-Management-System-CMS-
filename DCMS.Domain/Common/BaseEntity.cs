namespace DCMS.Domain.Common;

/// <summary>
/// Root base for every tracked entity in DCMS.
///
/// IMPORTANT — FIX-1:
///   CreatedAt and UpdatedAt are intentionally NOT initialized here.
///   Both timestamps are set exclusively inside ApplicationDbContext.SaveChangesAsync
///   via ChangeTracker so they reflect the actual moment of DB persistence,
///   not the moment the C# object was constructed (which can be arbitrarily earlier).
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
