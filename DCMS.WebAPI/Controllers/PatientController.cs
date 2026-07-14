using DCMS.Application.Exceptions;
using DCMS.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DCMS.Application.DTOs.Profile;
using DCMS.Application.DTOs.Common;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

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
    private readonly IPasswordHasher<User> _passwordHasher;

    public PatientController(IUnitOfWork uow, IPasswordHasher<User> passwordHasher)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
    private string GetUserRole() => User.FindFirstValue(System.Security.Claims.ClaimTypes.Role)!;

    private async Task EnsureDoctorPatientAccessAsync(int patientId, CancellationToken ct)
    {
        var role = GetUserRole();
        if (role == "Doctor" || role == "Owner")
        {
            var doctorId = GetUserId();
            var appts = await _uow.Appointments.FindAsync(a => a.PatientId == patientId && a.DoctorId == doctorId, ct);
            if (!appts.Any())
                throw new ForbiddenException("You are only allowed to access your own patients.");
        }
    }

    /// <summary>
    /// Search patients by name, phone, or ID.
    /// SRS §4.2, §4.3: Admin and Doctor can search for patients.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] PatientQueryDto query,
        CancellationToken ct)
    {
        var role = GetUserRole();
        int? doctorId = (role == "Doctor" || role == "Owner") ? GetUserId() : null;

        var paged = await _uow.Patients.GetQueriedPagedAsync(
            query.Page, query.PageSize,
            query.FullName, query.PhoneNumber, query.Id,
            query.BranchId, query.ServiceId,
            query.SortBy, query.SortDescending, doctorId, ct);

        var result = new PagedResultDto<PatientProfileResponseDto>
        {
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize,
            Items = paged.Items.Select(p => new PatientProfileResponseDto
            {
                Id = p.Id,
                FullName = p.FullName,
                Email = p.Email,
                Phone = p.Phone,
                DateOfBirth = p.DateOfBirth,
                MedicalHistory = p.MedicalHistory,
                BloodType = p.BloodType,
                Gender = p.Gender,
                Allergies = p.Allergies,
                IsFirstLogin = p.IsFirstLogin,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            }).ToList()
        };

        return Ok(result);
    }

    /// <summary>
    /// Get a patient's full profile including medical history.
    /// SRS §4.3: Doctors can view patient profiles during consultations.
    /// </summary>
    [HttpGet("{patientId:int}")]
    public async Task<IActionResult> GetProfile(int patientId, CancellationToken ct)
    {
        await EnsureDoctorPatientAccessAsync(patientId, ct);

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
            patient.BloodType,
            patient.Gender,
            patient.Allergies,
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
        await EnsureDoctorPatientAccessAsync(patientId, ct);

        var paged = await _uow.Reports.GetByPatientWithDetailsAsync(patientId, page, pageSize, null, ct);

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

    /// <summary>
    /// Create a new patient (Moderator/Admin/Owner only).
    /// </summary>
    [Authorize(Roles = "Admin,Owner")]
    [HttpPost]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientByAdminRequestDto dto, CancellationToken ct)
    {
        var existing = await _uow.Patients.GetByEmailAsync(dto.Email, ct);
        if (existing != null)
            throw new ConflictException("Email is already registered.");

        var patient = new Patient
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            MedicalHistory = dto.MedicalHistory,
            BloodType = dto.BloodType,
            Gender = dto.Gender,
            Allergies = dto.Allergies,
            Role = UserRole.Patient,
            IsFirstLogin = true, // Force them to change password when they first log in
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        patient.PasswordHash = _passwordHasher.HashPassword(patient, dto.Password);

        await _uow.Patients.AddAsync(patient, ct);
        await _uow.SaveChangesAsync(ct);

        return Ok(new { patient.Id, patient.FullName });
    }

    /// <summary>
    /// Update a patient (Moderator/Admin/Owner only).
    /// </summary>
    [Authorize(Roles = "Admin,Owner")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePatient(int id, [FromBody] UpdatePatientRequestDto dto, CancellationToken ct)
    {
        var patient = await _uow.Patients.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Patient {id} not found.");

        patient.FullName = dto.FullName;
        patient.Phone = dto.Phone;
        patient.DateOfBirth = dto.DateOfBirth;
        patient.MedicalHistory = dto.MedicalHistory;
        patient.BloodType = dto.BloodType;
        patient.Gender = dto.Gender;
        patient.Allergies = dto.Allergies;
        patient.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        return NoContent();
    }

    /// <summary>
    /// Soft delete a patient (Moderator/Admin/Owner only).
    /// </summary>
    [Authorize(Roles = "Admin,Owner")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> SoftDeletePatient(int id, CancellationToken ct)
    {
        var patient = await _uow.Patients.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Patient {id} not found.");

        patient.IsActive = false;
        patient.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        return NoContent();
    }
}
