using DCMS.Application.DTOs.DentalChart;
using DCMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DCMS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DentalChartController : ControllerBase
{
    private readonly IDentalChartService _dentalChartService;

    public DentalChartController(IDentalChartService dentalChartService)
    {
        _dentalChartService = dentalChartService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = "Doctor,Patient")]
    [HttpGet("{patientId:int}")]
    public async Task<IActionResult> Get(int patientId, CancellationToken ct)
    {
        var result = await _dentalChartService.GetByPatientIdAsync(patientId, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Doctor")]
    [HttpPut("{patientId:int}/notes")]
    public async Task<IActionResult> UpdateNotes(int patientId, [FromBody] UpdateDentalChartRequestDto dto, CancellationToken ct)
    {
        var doctorId = GetUserId();
        var result = await _dentalChartService.UpdateChartNotesAsync(patientId, dto, doctorId, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Doctor")]
    [HttpPut("{patientId:int}/tooth")]
    public async Task<IActionResult> UpsertToothRecord(int patientId, [FromBody] UpsertToothRecordRequestDto dto, CancellationToken ct)
    {
        var doctorId = GetUserId();
        var result = await _dentalChartService.UpsertToothRecordAsync(patientId, dto, doctorId, ct);
        return Ok(result);
    }
}
