using DCMS.Application.DTOs.Common;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DCMS.WebAPI.Controllers;

/// <summary>
/// Admin-specific operations: patient search, booking filters, cancelled/absent reports.
/// SRS §4.2
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin,Owner")]
public class AdminController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IAppointmentService _appointmentService;

    public AdminController(IUnitOfWork uow, IAppointmentService appointmentService)
    {
        _uow = uow;
        _appointmentService = appointmentService;
    }

    // ── Patient search (SRS §4.2: SearchPatient by ID/phone/name) ──────────

    [HttpGet("patients/search")]
    public async Task<IActionResult> SearchPatients(
        [FromQuery] string? name,
        [FromQuery] string? phone,
        [FromQuery] int? id,
        CancellationToken ct)
    {
        var patients = await _uow.Patients.GetAllAsync(ct);

        var filtered = patients.Where(p =>
            (id == null || p.Id == id) &&
            (name == null || p.FullName.Contains(name, StringComparison.OrdinalIgnoreCase)) &&
            (phone == null || (p.Phone != null && p.Phone.Contains(phone)))
        ).Select(p => new
        {
            p.Id,
            p.FullName,
            p.Email,
            p.Phone,
            p.DateOfBirth,
            p.IsActive,
            p.MedicalHistory
        });

        return Ok(filtered);
    }

    [HttpGet("patients/{patientId:int}")]
    public async Task<IActionResult> GetPatient(int patientId, CancellationToken ct)
    {
        var patient = await _uow.Patients.GetByIdAsync(patientId, ct);
        if (patient == null) throw new NotFoundException($"Patient {patientId} not found.");
        return Ok(new
        {
            patient.Id,
            patient.FullName,
            patient.Email,
            patient.Phone,
            patient.DateOfBirth,
            patient.IsActive,
            patient.MedicalHistory
        });
    }

    // ── Appointment filters (SRS §4.2) ──────────────────────────────────────

    [HttpGet("appointments/cancelled")]
    public async Task<IActionResult> GetCancelledAppointments(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var paged = await _uow.Appointments.GetPagedAsync(page, pageSize, a =>
            a.Status == DCMS.Domain.Enums.AppointmentStatus.Cancelled &&
            (from == null || a.Date >= from) &&
            (to == null || a.Date <= to), ct);

        return Ok(new PagedResultDto<object>
        {
            Items = paged.Items.Select(a => (object)new
            {
                a.Id,
                PatientName = a.Patient?.FullName,
                DoctorName = a.Doctor?.FullName,
                a.Date,
                a.StartTime,
                a.CancelledAt,
                a.CancelledBy
            }).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        });
    }

    [HttpGet("appointments/absent")]
    public async Task<IActionResult> GetAbsentAppointments(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var paged = await _uow.Appointments.GetPagedAsync(page, pageSize, a =>
            a.AttendanceStatus == DCMS.Domain.Enums.AttendanceStatus.Absent &&
            (from == null || a.Date >= from) &&
            (to == null || a.Date <= to), ct);

        return Ok(new PagedResultDto<object>
        {
            Items = paged.Items.Select(a => (object)new
            {
                a.Id,
                PatientName = a.Patient?.FullName,
                DoctorName = a.Doctor?.FullName,
                a.Date,
                a.StartTime
            }).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        });
    }

    [HttpGet("appointments/search")]
    public async Task<IActionResult> SearchAppointments(
        [FromQuery] int? patientId,
        [FromQuery] int? doctorId,
        [FromQuery] string? status,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] bool? isUrgent,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var parsedStatus = status != null && Enum.TryParse<DCMS.Domain.Enums.AppointmentStatus>(status, true, out var s) ? s : (DCMS.Domain.Enums.AppointmentStatus?)null;

        var paged = await _uow.Appointments.GetPagedAsync(page, pageSize, a =>
            (patientId == null || a.PatientId == patientId) &&
            (doctorId == null || a.DoctorId == doctorId) &&
            (parsedStatus == null || a.Status == parsedStatus) &&
            (from == null || a.Date >= from) &&
            (to == null || a.Date <= to) &&
            (isUrgent == null || a.IsUrgent == isUrgent), ct);

        return Ok(new PagedResultDto<object>
        {
            Items = paged.Items.Select(a => (object)new
            {
                a.Id,
                PatientName = a.Patient?.FullName,
                DoctorName = a.Doctor?.FullName,
                BranchName = a.Branch?.Name,
                ServiceName = a.Service?.Name,
                a.Date,
                a.StartTime,
                a.EndTime,
                Status = a.Status.ToString(),
                a.IsUrgent
            }).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        });
    }
}
