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

    [Authorize(Roles = "Doctor")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionRequestDto dto, CancellationToken ct)
    {
        var result = await _prescriptionService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Patient,Doctor,Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _prescriptionService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Patient,Doctor,Admin")]
    [HttpGet("by-report/{reportId:int}")]
    public async Task<IActionResult> GetByReport(int reportId, CancellationToken ct)
    {
        var result = await _prescriptionService.GetByReportIdAsync(reportId, ct);
        return Ok(result);
    }
}
