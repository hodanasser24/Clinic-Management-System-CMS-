using DCMS.Application.DTOs.Prescriptions;
using DCMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DCMS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrescriptionController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;

    public PrescriptionController(IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
    private string GetUserRole() => User.FindFirstValue(System.Security.Claims.ClaimTypes.Role)!;

    private int? GetCallerIdFilter()
    {
        var role = GetUserRole();
        return (role == "Doctor" || role == "Owner") ? GetUserId() : null;
    }

    // ── Read ───────────────────────────────────────────────────────────────────

    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _prescriptionService.GetByIdAsync(id, GetCallerIdFilter(), ct);
        return Ok(result);
    }

    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("by-report/{reportId:int}")]
    public async Task<IActionResult> GetByReport(int reportId, CancellationToken ct)
    {
        var result = await _prescriptionService.GetByReportIdAsync(reportId, GetCallerIdFilter(), ct);
        return Ok(result);
    }

    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("by-patient/{patientId:int}")]
    public async Task<IActionResult> GetByPatient(
        int patientId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _prescriptionService.GetByPatientAsync(patientId, page, pageSize, GetCallerIdFilter(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Export a prescription as a downloadable PDF file.
    /// </summary>
    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("{id:int}/export")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Export(int id, CancellationToken ct)
    {
        var bytes = await _prescriptionService.ExportPdfAsync(id, ct);
        return File(bytes, "application/pdf", $"prescription-{id}.pdf");
    }

    // ── Write ──────────────────────────────────────────────────────────────────

    [Authorize(Roles = "Doctor,Owner")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePrescriptionRequestDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _prescriptionService.CreateAsync(dto, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Doctor,Owner")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] CreatePrescriptionRequestDto dto, CancellationToken ct)
    {
        var userId = int.Parse(
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _prescriptionService.UpdateAsync(id, userId, dto, ct);
        return Ok(result);
    }
}
