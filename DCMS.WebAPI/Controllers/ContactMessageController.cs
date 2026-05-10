using DCMS.Application.DTOs;
using DCMS.Application.Interfaces;
using DCMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ContactMessageController : ControllerBase
{
    private readonly IContactMessageService _service;
    private readonly ICurrentUserService    _currentUser;

    public ContactMessageController(
        IContactMessageService service,
        ICurrentUserService    currentUser)
    {
        _service     = service;
        _currentUser = currentUser;
    }

    // ── Public ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Submit a new contact message (public / guest access).
    /// POST /api/contactmessage
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ContactMessageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateContactMessageRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // ── Staff (Admin / Owner) ─────────────────────────────────────────────────

    /// <summary>
    /// List all contact messages with optional filters.
    /// GET /api/contactmessage
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(typeof(PagedContactMessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ContactMessageFilterRequest filter,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get messages filtered by ContactMessageType.
    /// Supports additional secondary filters (status, search, date range, pagination).
    ///
    /// GET /api/contactmessage/by-type?type=Complaint&amp;status=Pending&amp;page=1&amp;pageSize=20
    /// </summary>
    [HttpGet("by-type")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(typeof(PagedContactMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByType(
        [FromQuery] ContactMessageFilterRequest filter,
        CancellationToken ct)
    {
        // Guard: type must be supplied for this dedicated endpoint
        if (!filter.Type.HasValue)
            return BadRequest(new { message = "Query parameter 'type' is required for this endpoint. Use GET /api/contactmessage to retrieve all types." });

        if (!Enum.IsDefined(typeof(ContactMessageType), filter.Type.Value))
            return BadRequest(new { message = $"Invalid type value '{filter.Type}'. Valid values: {string.Join(", ", Enum.GetNames<ContactMessageType>())}" });

        var result = await _service.GetByTypeAsync(filter, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get a single contact message by ID (marks it as Read automatically).
    /// GET /api/contactmessage/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(typeof(ContactMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Reply to a contact message (sets status → Replied).
    /// POST /api/contactmessage/{id}/reply
    /// </summary>
    [HttpPost("{id:int}/reply")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(typeof(ContactMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reply(
        int id,
        [FromBody] ReplyContactMessageRequest request,
        CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User identity not found.");

        var result = await _service.ReplyAsync(id, request, userId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Update the status of a contact message (e.g. Pending → InProgress → Closed).
    /// PATCH /api/contactmessage/{id}/status
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(typeof(ContactMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateContactMessageStatusRequest request,
        CancellationToken ct)
    {
        var result = await _service.UpdateStatusAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Soft-archive a contact message (hidden from normal lists).
    /// DELETE /api/contactmessage/{id}/archive
    /// </summary>
    [HttpDelete("{id:int}/archive")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(int id, CancellationToken ct)
    {
        await _service.ArchiveAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Permanently delete a contact message (Owner only).
    /// DELETE /api/contactmessage/{id}
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Owner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
