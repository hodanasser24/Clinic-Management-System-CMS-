using DCMS.Application.DTOs.Reports;
using DCMS.Application.Interfaces;
using DCMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DCMS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    private int      GetUserId()   => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private UserRole GetUserRole() => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

    // ── Read ───────────────────────────────────────────────────────────────────

    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _reportService.GetByIdAsync(id, GetUserRole(), ct);
        return Ok(result);
    }

    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("by-patient/{patientId:int}")]
    public async Task<IActionResult> GetByPatient(
        int patientId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _reportService.GetByPatientAsync(patientId, GetUserRole(), page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>All reports written by a specific doctor — Doctor/Admin/Owner only.</summary>
    [Authorize(Roles = "Doctor,Admin,Owner")]
    [HttpGet("by-doctor/{doctorId:int}")]
    public async Task<IActionResult> GetByDoctor(
        int doctorId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _reportService.GetByDoctorAsync(doctorId, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Compare two reports for the same patient — clinical timeline diff.
    /// Returns tooth status changes, diagnosis change summary, improved/worsened counts.
    /// </summary>
    [Authorize(Roles = "Doctor,Owner")]
    [HttpGet("compare")]
    public async Task<IActionResult> Compare(
        [FromQuery] int reportId1, [FromQuery] int reportId2,
        CancellationToken ct)
    {
        var result = await _reportService.CompareAsync(reportId1, reportId2, ct);
        return Ok(result);
    }

    // ── Write ──────────────────────────────────────────────────────────────────

    [Authorize(Roles = "Doctor")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateReportRequestDto dto, CancellationToken ct)
    {
        // Doctor creating the report is always the authenticated user
        dto.DoctorId = GetUserId();
        var result = await _reportService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Doctor")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateReportRequestDto dto, CancellationToken ct)
    {
        var result = await _reportService.UpdateAsync(id, dto, GetUserId(), ct);
        return Ok(result);
    }
}
