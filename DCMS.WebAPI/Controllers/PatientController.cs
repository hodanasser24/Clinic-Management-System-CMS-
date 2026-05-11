using DCMS.Application.Exceptions;
using DCMS.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.WebAPI.Controllers;

/// <summary>
/// Doctor-accessible patient profile and history endpoints.
/// Use case: Doctor reviews a patient's profile and medical history before/after a consultation.
/// </summary>
[ApiController]
[Route("api/patients")]
[Authorize(Roles = "Doctor,Admin,Owner")]
public class PatientController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public PatientController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    /// <summary>
    /// Search patients by name, phone, or ID.
    /// SRS §4.2, §4.3: Admin and Doctor can search for patients.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? name,
        [FromQuery] string? phone,
        [FromQuery] int?    id,
        CancellationToken   ct)
    {
        var all = await _uow.Patients.GetAllAsync(ct);

        var filtered = all
            .Where(p =>
                (id    == null || p.Id    == id)    &&
                (name  == null || p.FullName.Contains(name,  StringComparison.OrdinalIgnoreCase)) &&
                (phone == null || (p.Phone != null && p.Phone.Contains(phone))))
            .Select(p => new
            {
                p.Id,
                p.FullName,
                p.Email,
                p.Phone,
                p.DateOfBirth,
                p.IsActive
            });

        return Ok(filtered);
    }

    /// <summary>
    /// Get a patient's full profile including medical history.
    /// SRS §4.3: Doctors can view patient profiles during consultations.
    /// </summary>
    [HttpGet("{patientId:int}")]
    public async Task<IActionResult> GetProfile(int patientId, CancellationToken ct)
    {
        var patient = await _uow.Patients.GetByIdAsync(patientId, ct)
            ?? throw new NotFoundException($"Patient {patientId} not found.");

        return Ok(new
        {
            patient.Id,
            patient.FullName,
            patient.Email,
            patient.Phone,
            patient.DateOfBirth,
            patient.IsActive,
            patient.MedicalHistory,
            patient.IsFirstLogin,
            patient.CreatedAt
        });
    }

    /// <summary>
    /// Get all reports for a patient — doctor sees DoctorReportResponseDto (with InternalNotes).
    /// SRS §4.3: Doctor can review full patient medical record.
    /// </summary>
    [HttpGet("{patientId:int}/reports")]
    public async Task<IActionResult> GetReports(
        int patientId,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var paged = await _uow.Reports.GetByPatientWithDetailsAsync(patientId, page, pageSize, ct);

        return Ok(new
        {
            paged.TotalCount,
            paged.Page,
            paged.PageSize,
            paged.TotalPages,
            Items = paged.Items.Select(r => new
            {
                r.Id,
                r.AppointmentId,
                r.Diagnosis,
                r.Treatment,
                r.Notes,
                r.InternalNotes,     // Visible to Doctor/Owner (BR-57)
                r.CaseStatus,
                r.FollowUpInstructions,
                r.TreatmentPlan,
                r.DietInstructions,
                r.AllowedFood,
                r.RestrictedFood,
                r.HomeCareInstructions,
                r.CreatedAt
            })
        });
    }
}
