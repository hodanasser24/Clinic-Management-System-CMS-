using DCMS.Application.DTOs.Contacts;

namespace DCMS.Application.Interfaces;

public interface IContactMessageService
{
    // Public
    Task<ContactMessageResponseDto> CreateAsync(CreateContactMessageRequestDto request, CancellationToken ct = default);

    // Admin / Owner – list & filter
    Task<PagedContactMessageResponseDto> GetAllAsync(ContactMessageFilterRequestDto filter, CancellationToken ct = default);

    /// <summary>
    /// Primary GetByType endpoint – returns paginated messages filtered by
    /// ContactMessageType (and optional secondary filters).
    /// </summary>
    Task<PagedContactMessageResponseDto> GetByTypeAsync(ContactMessageFilterRequestDto filter, CancellationToken ct = default);

    Task<ContactMessageResponseDto> GetByIdAsync(int id, CancellationToken ct = default);

    // Mutation
    Task<ContactMessageResponseDto> ReplyAsync(int id, ReplyContactMessageRequestDto request, int repliedByUserId, CancellationToken ct = default);
    Task<ContactMessageResponseDto> UpdateStatusAsync(int id, UpdateContactMessageStatusRequestDto request, CancellationToken ct = default);
    Task ArchiveAsync(int id, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
