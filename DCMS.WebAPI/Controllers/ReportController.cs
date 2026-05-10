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

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private UserRole GetUserRole() => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

    [Authorize(Roles = "Doctor")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReportRequestDto dto, CancellationToken ct)
    {
        // Note: CreateAsync in service might need the DoctorId from the token. 
        // If CreateReportRequestDto doesn't have DoctorId, we should set it or the service should use it.
        // Assuming service implementation handles it or DTO has it.
        var result = await _reportService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Doctor")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReportRequestDto dto, CancellationToken ct)
    {
        var doctorId = GetUserId();
        var result = await _reportService.UpdateAsync(id, dto, doctorId, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var role = GetUserRole();
        var result = await _reportService.GetByIdAsync(id, role, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("by-patient/{patientId:int}")]
    public async Task<IActionResult> GetByPatient(int patientId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var role = GetUserRole();
        var result = await _reportService.GetByPatientAsync(patientId, role, page, pageSize, ct);
        return Ok(result);
    }
}
