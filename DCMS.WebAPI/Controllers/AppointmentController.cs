using DCMS.Application.DTOs.Appointments;
using DCMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DCMS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = "Patient")]
    [HttpPost]
    public async Task<IActionResult> Book([FromBody] AppointmentRequestDto dto, CancellationToken ct)
    {
        dto.PatientId = GetUserId();
        var result = await _appointmentService.BookAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _appointmentService.GetAllAsync(page, pageSize, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _appointmentService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("by-patient/{patientId:int}")]
    public async Task<IActionResult> GetByPatient(int patientId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _appointmentService.GetByPatientAsync(patientId, page, pageSize, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("by-doctor/{doctorId:int}")]
    public async Task<IActionResult> GetByDoctor(int doctorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _appointmentService.GetByDoctorAsync(doctorId, page, pageSize, ct);
        return Ok(result);
    }

    // SRS §4.3, §4.4 — Doctor and Owner can view urgent appointments
    [Authorize(Roles = "Doctor,Admin,Owner")]
    [HttpGet("urgent")]
    public async Task<IActionResult> GetUrgent([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _appointmentService.GetUrgentAsync(page, pageSize, ct);
        return Ok(result);
    }

    // SRS §4.3 — Doctor: FilterUrgentAppointmentsByDate()
    [Authorize(Roles = "Doctor,Admin,Owner")]
    [HttpGet("urgent/by-date-range")]
    public async Task<IActionResult> GetUrgentByDateRange([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _appointmentService.GetUrgentByDateRangeAsync(from, to, page, pageSize, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id, [FromBody] ConfirmAppointmentRequestDto dto, CancellationToken ct)
    {
        dto.AdminId = GetUserId();
        var result = await _appointmentService.ConfirmAsync(id, dto, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectAppointmentRequestDto dto, CancellationToken ct)
    {
        dto.AdminId = GetUserId();
        var result = await _appointmentService.RejectAsync(id, dto, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Patient,Admin")]
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelAppointmentRequestDto dto, CancellationToken ct)
    {
        var result = await _appointmentService.CancelAsync(id, GetUserId(), dto, ct);
        return Ok(result);
    }

    // SRS §4.1, §4.2 — Patient and Admin can reschedule
    [Authorize(Roles = "Patient,Admin")]
    [HttpPut("{id:int}/reschedule")]
    public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleAppointmentRequestDto dto, CancellationToken ct)
    {
        var result = await _appointmentService.RescheduleAsync(id, GetUserId(), dto, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Doctor")]
    [HttpPut("{id:int}/mark-urgent")]
    public async Task<IActionResult> MarkUrgent(int id, [FromBody] MarkUrgentRequestDto dto, CancellationToken ct)
    {
        dto.DoctorId = GetUserId();
        var result = await _appointmentService.MarkUrgentAsync(id, dto, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Doctor")]
    [HttpPut("{id:int}/unmark-urgent")]
    public async Task<IActionResult> UnmarkUrgent(int id, CancellationToken ct)
    {
        var result = await _appointmentService.UnmarkUrgentAsync(id, GetUserId(), ct);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/mark-attendance")]
    public async Task<IActionResult> MarkAttendance(int id, [FromBody] MarkAttendanceRequestDto dto, CancellationToken ct)
    {
        var result = await _appointmentService.MarkAttendanceAsync(id, dto, ct);
        return Ok(result);
    }
}
