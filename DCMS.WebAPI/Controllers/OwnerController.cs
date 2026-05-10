using DCMS.Application.DTOs.Owner;
using DCMS.Application.DTOs.Schedules;
using DCMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DCMS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner")]
public class OwnerController : ControllerBase
{
    private readonly IOwnerService _ownerService;

    public OwnerController(IOwnerService ownerService)
    {
        _ownerService = ownerService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── Staff Account Management ───────────────────────────────────────────────

    [HttpPost("doctors")]
    public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorAccountRequestDto dto, CancellationToken ct)
    {
        var result = await _ownerService.CreateDoctorAccountAsync(dto, ct);
        return CreatedAtAction(nameof(GetAllDoctors), new { id = result.Id }, result);
    }

    [HttpPost("admins")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminAccountRequestDto dto, CancellationToken ct)
    {
        var result = await _ownerService.CreateAdminAccountAsync(dto, ct);
        return CreatedAtAction(nameof(GetAllAdmins), new { id = result.Id }, result);
    }

    [HttpGet("doctors")]
    public async Task<IActionResult> GetAllDoctors([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _ownerService.GetAllDoctorsAsync(page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("admins")]
    public async Task<IActionResult> GetAllAdmins([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _ownerService.GetAllAdminsAsync(page, pageSize, ct);
        return Ok(result);
    }

    [HttpPut("accounts/{userId:int}/deactivate")]
    public async Task<IActionResult> DeactivateAccount(int userId, [FromBody] DeactivateAccountRequestDto dto, CancellationToken ct)
    {
        var result = await _ownerService.DeactivateAccountAsync(userId, dto, ct);
        return Ok(result);
    }

    [HttpPut("accounts/{userId:int}/reactivate")]
    public async Task<IActionResult> ReactivateAccount(int userId, CancellationToken ct)
    {
        var result = await _ownerService.ReactivateAccountAsync(userId, ct);
        return Ok(result);
    }

    // ── Doctor Profile Management ─────────────────────────────────────────────

    [HttpPut("doctors/{doctorId:int}/profile")]
    public async Task<IActionResult> UpdateDoctorProfile(int doctorId, [FromBody] UpdateDoctorProfileRequestDto dto, CancellationToken ct)
    {
        var result = await _ownerService.UpdateDoctorProfileAsync(doctorId, dto, ct);
        return Ok(result);
    }

    [HttpPut("doctors/{doctorId:int}/photo")]
    public async Task<IActionResult> UpdateDoctorPhoto(int doctorId, [FromBody] UpdateDoctorPhotoRequestDto dto, CancellationToken ct)
    {
        var result = await _ownerService.UpdateDoctorPhotoAsync(doctorId, dto, ct);
        return Ok(result);
    }

    // ── Offer Management — BR-58: Owner-only ──────────────────────────────────

    [HttpGet("offers")]
    public async Task<IActionResult> GetAllOffers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _ownerService.GetAllOffersAsync(page, pageSize, ct);
        return Ok(result);
    }

    [HttpPut("offers/{offerId:int}/activate")]
    public async Task<IActionResult> ActivateOffer(int offerId, CancellationToken ct)
    {
        var result = await _ownerService.ActivateOfferAsync(offerId, ct);
        return Ok(result);
    }

    [HttpPut("offers/{offerId:int}/deactivate")]
    public async Task<IActionResult> DeactivateOffer(int offerId, CancellationToken ct)
    {
        var result = await _ownerService.DeactivateOfferAsync(offerId, ct);
        return Ok(result);
    }

    // ── Schedule Change Request Approvals — BR-6 ──────────────────────────────

    [HttpGet("schedule-requests")]
    public async Task<IActionResult> GetPendingScheduleRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _ownerService.GetPendingScheduleRequestsAsync(page, pageSize, ct);
        return Ok(result);
    }

    [HttpPatch("schedule-requests/{requestId:int}/approve")]
    public async Task<IActionResult> ApproveScheduleRequest(int requestId, CancellationToken ct)
    {
        var ownerId = GetUserId();
        var result = await _ownerService.ApproveScheduleRequestAsync(requestId, ownerId, ct);
        return Ok(result);
    }

    [HttpPatch("schedule-requests/{requestId:int}/reject")]
    public async Task<IActionResult> RejectScheduleRequest(int requestId, [FromBody] RejectRequestBody body, CancellationToken ct)
    {
        var ownerId = GetUserId();
        var result = await _ownerService.RejectScheduleRequestAsync(requestId, ownerId, body.Reason, ct);
        return Ok(result);
    }
}

public record RejectRequestBody(string? Reason);
