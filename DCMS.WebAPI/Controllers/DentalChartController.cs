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
    private readonly DCMS.Domain.Interfaces.IUnitOfWork _uow;

    public DentalChartController(IDentalChartService dentalChartService, DCMS.Domain.Interfaces.IUnitOfWork uow)
    {
        _dentalChartService = dentalChartService;
        _uow = uow;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetUserRole() => User.FindFirstValue(ClaimTypes.Role)!;

    private async Task EnsureDoctorPatientAccessAsync(int patientId, CancellationToken ct)
    {
        var role = GetUserRole();
        if (role == "Doctor" || role == "Owner")
        {
            var doctorId = GetUserId();
            var appts = await _uow.Appointments.FindAsync(a => a.PatientId == patientId && a.DoctorId == doctorId, ct);
            if (!appts.Any())
                throw new DCMS.Application.Exceptions.ForbiddenException("You are only allowed to access your own patients.");
        }
    }

    [Authorize(Roles = "Doctor,Patient,Owner")]
    [HttpGet("{patientId:int}")]
    public async Task<IActionResult> Get(int patientId, CancellationToken ct)
    {
        await EnsureDoctorPatientAccessAsync(patientId, ct);
        var result = await _dentalChartService.GetByPatientIdAsync(patientId, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Doctor,Owner")]
    [HttpPut("{patientId:int}/notes")]
    public async Task<IActionResult> UpdateNotes(int patientId, [FromBody] UpdateDentalChartRequestDto dto, CancellationToken ct)
    {
        await EnsureDoctorPatientAccessAsync(patientId, ct);
        var doctorId = GetUserId();
        var result = await _dentalChartService.UpdateChartNotesAsync(patientId, dto, doctorId, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Doctor,Owner")]
    [HttpPut("{patientId:int}/tooth")]
    public async Task<IActionResult> UpsertToothRecord(int patientId, [FromBody] UpsertToothRecordRequestDto dto, CancellationToken ct)
    {
        await EnsureDoctorPatientAccessAsync(patientId, ct);
        var doctorId = GetUserId();
        var result = await _dentalChartService.UpsertToothRecordAsync(patientId, dto, doctorId, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Doctor,Owner")]
    [HttpPut("{patientId:int}/bulk")]
    public async Task<IActionResult> BulkUpsertToothRecords(int patientId, [FromBody] BulkUpsertToothRecordsRequestDto dto, CancellationToken ct)
    {
        await EnsureDoctorPatientAccessAsync(patientId, ct);
        var doctorId = GetUserId();
        var result = await _dentalChartService.BulkUpsertToothRecordsAsync(patientId, dto, doctorId, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Doctor,Owner")]
    [HttpGet("{patientId:int}/export")]
    public async Task<IActionResult> ExportPdf(int patientId, CancellationToken ct)
    {
        await EnsureDoctorPatientAccessAsync(patientId, ct);
        var doctorId = GetUserId();
        var pdfBytes = await _dentalChartService.ExportPdfAsync(patientId, doctorId, ct);
        return File(pdfBytes, "application/pdf", $"DentalChart_{patientId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
    }
}
