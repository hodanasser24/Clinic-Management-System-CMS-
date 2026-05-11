using DCMS.Application.DTOs.Prescriptions;
using DCMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    // ── Read ───────────────────────────────────────────────────────────────────

    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _prescriptionService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("by-report/{reportId:int}")]
    public async Task<IActionResult> GetByReport(int reportId, CancellationToken ct)
    {
        var result = await _prescriptionService.GetByReportIdAsync(reportId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Export prescription as a downloadable text/PDF file.
    /// Previously implemented in IPrescriptionService but no HTTP endpoint existed.
    /// Patient and Doctor can download their own prescriptions.
    /// </summary>
    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("{id:int}/export")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Export(int id, CancellationToken ct)
    {
        var bytes = await _prescriptionService.ExportPdfAsync(id, ct);
        return File(bytes, "application/octet-stream", $"prescription-{id}.txt");
    }

    // ── Write ──────────────────────────────────────────────────────────────────

    [Authorize(Roles = "Doctor")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePrescriptionRequestDto dto, CancellationToken ct)
    {
        var result = await _prescriptionService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Doctor")]
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
