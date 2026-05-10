using DCMS.Application.DTOs.Schedules;
using DCMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DCMS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public ScheduleController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = "Admin,Owner")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateScheduleRequestDto dto, CancellationToken ct)
    {
        var result = await _scheduleService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetByDoctor), new { doctorId = result.DoctorId }, result);
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateScheduleRequestDto dto, CancellationToken ct)
    {
        var result = await _scheduleService.UpdateAsync(id, dto, ct);
        return Ok(result);
    }

    [HttpGet("by-doctor/{doctorId:int}")]
    public async Task<IActionResult> GetByDoctor(int doctorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _scheduleService.GetByDoctorAsync(doctorId, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("by-branch/{branchId:int}")]
    public async Task<IActionResult> GetByBranch(int branchId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _scheduleService.GetByBranchAsync(branchId, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{scheduleId:int}/available-slots")]
    public async Task<IActionResult> GetAvailableSlots(int scheduleId, [FromQuery] DateOnly date, CancellationToken ct)
    {
        var result = await _scheduleService.GetAvailableTimeSlotsAsync(scheduleId, date, ct);
        return Ok(result);
    }

    // SRS §4.2: Admin submits schedule change requests
    [Authorize(Roles = "Admin")]
    [HttpPost("change-request")]
    public async Task<IActionResult> SubmitChangeRequest([FromBody] CreateScheduleChangeRequestDto dto, CancellationToken ct)
    {
        dto.RequestingAdminId = GetUserId();
        var result = await _scheduleService.SubmitChangeRequestAsync(dto, ct);
        return Ok(result);
    }

    // Doctor approves their own schedule change
    [Authorize(Roles = "Doctor")]
    [HttpPut("change-request/{id:int}/approve-doctor")]
    public async Task<IActionResult> ApproveByDoctor(int id, CancellationToken ct)
    {
        var result = await _scheduleService.ApproveChangeRequestByDoctorAsync(id, GetUserId(), ct);
        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("change-request/{id:int}/approve-owner")]
    public async Task<IActionResult> ApproveByOwner(int id, CancellationToken ct)
    {
        var result = await _scheduleService.ApproveChangeRequestByOwnerAsync(id, GetUserId(), ct);
        return Ok(result);
    }

    [Authorize(Roles = "Doctor,Owner")]
    [HttpPut("change-request/{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, CancellationToken ct)
    {
        var result = await _scheduleService.RejectChangeRequestAsync(id, GetUserId(), ct);
        return Ok(result);
    }
}
