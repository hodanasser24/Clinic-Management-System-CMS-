namespace DCMS.Application.Interfaces;

/// <summary>
/// Provides access to the currently authenticated user's identity claims.
/// Resolved via HttpContext in the Infrastructure layer (CurrentUserService).
/// </summary>
public interface ICurrentUserService
{
    int?    UserId          { get; }
    string? Email           { get; }
    string? Role            { get; }
    bool    IsAuthenticated { get; }
}
