using DCMS.Application.DTOs;

namespace DCMS.Application.Interfaces;

public interface IContactMessageService
{
    // Public
    Task<ContactMessageResponse> CreateAsync(CreateContactMessageRequest request, CancellationToken ct = default);

    // Admin / Owner – list & filter
    Task<PagedContactMessageResponse> GetAllAsync(ContactMessageFilterRequest filter, CancellationToken ct = default);

    /// <summary>
    /// Primary GetByType endpoint – returns paginated messages filtered by
    /// ContactMessageType (and optional secondary filters).
    /// </summary>
    Task<PagedContactMessageResponse> GetByTypeAsync(ContactMessageFilterRequest filter, CancellationToken ct = default);

    Task<ContactMessageResponse> GetByIdAsync(int id, CancellationToken ct = default);

    // Mutation
    Task<ContactMessageResponse> ReplyAsync(int id, ReplyContactMessageRequest request, int repliedByUserId, CancellationToken ct = default);
    Task<ContactMessageResponse> UpdateStatusAsync(int id, UpdateContactMessageStatusRequest request, CancellationToken ct = default);
    Task ArchiveAsync(int id, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
