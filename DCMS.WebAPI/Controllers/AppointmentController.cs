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

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── Read endpoints ─────────────────────────────────────────────────────────

    [Authorize(Roles = "Admin,Owner")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] AppointmentQueryDto query,
        CancellationToken ct = default)
    {
        var result = await _appointmentService.GetAllAsync(query, ct);
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
    public async Task<IActionResult> GetByPatient(
        int patientId,
        [FromQuery] AppointmentQueryDto query,
        CancellationToken ct = default)
    {
        var result = await _appointmentService.GetByPatientAsync(patientId, query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Upcoming (Pending/Confirmed) appointments for a patient — date ≥ today.
    /// Use case: Patient views their next appointments.
    /// </summary>
    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("upcoming/by-patient/{patientId:int}")]
    public async Task<IActionResult> GetUpcoming(
        int patientId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _appointmentService.GetUpcomingByPatientAsync(patientId, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Completed/Cancelled/Rejected appointments for a patient — visit history.
    /// Use case: Patient views their visit history; Doctor reviews patient history.
    /// </summary>
    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("history/by-patient/{patientId:int}")]
    public async Task<IActionResult> GetHistory(
        int patientId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _appointmentService.GetHistoryByPatientAsync(patientId, page, pageSize, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Patient,Doctor,Admin,Owner")]
    [HttpGet("by-doctor/{doctorId:int}")]
    public async Task<IActionResult> GetByDoctor(
        int doctorId,
        [FromQuery] AppointmentQueryDto query,
        CancellationToken ct = default)
    {
        var result = await _appointmentService.GetByDoctorAsync(doctorId, query, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Doctor,Admin,Owner")]
    [HttpGet("urgent")]
    public async Task<IActionResult> GetUrgent(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _appointmentService.GetUrgentAsync(page, pageSize, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Doctor,Admin,Owner")]
    [HttpGet("urgent/by-date-range")]
    public async Task<IActionResult> GetUrgentByDateRange(
        [FromQuery] DateOnly from, [FromQuery] DateOnly to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _appointmentService.GetUrgentByDateRangeAsync(from, to, page, pageSize, ct);
        return Ok(result);
    }

    // ── Mutation endpoints ─────────────────────────────────────────────────────

    [Authorize(Roles = "Patient,Admin,Owner")]
    [HttpPost]
    public async Task<IActionResult> Book(
        [FromBody] AppointmentRequestDto dto, CancellationToken ct)
    {
        if (User.IsInRole("Patient"))
            dto.PatientId = GetUserId();
        var result = await _appointmentService.BookAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] AppointmentRequestDto dto, CancellationToken ct)
    {
        var result = await _appointmentService.UpdateAsync(id, dto, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(
        int id, CancellationToken ct)
    {
        var adminId = GetUserId();
        var result = await _appointmentService.ConfirmAsync(id, adminId, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/reject")]
    public async Task<IActionResult> Reject(
        int id, CancellationToken ct)
    {
        var adminId = GetUserId();
        var result = await _appointmentService.RejectAsync(id, adminId, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Patient,Owner")]
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(
        int id, [FromBody] CancelAppointmentRequestDto dto, CancellationToken ct)
    {
        Console.WriteLine("=== CANCEL ENDPOINT HIT ===");
        Console.WriteLine($"IsAuthenticated: {User.Identity?.IsAuthenticated}");
        Console.WriteLine($"IsInRole(Owner): {User.IsInRole("Owner")}");
        Console.WriteLine($"IsInRole(Doctor): {User.IsInRole("Doctor")}");
        Console.WriteLine($"IsInRole(Admin): {User.IsInRole("Admin")}");
        Console.WriteLine($"Claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");
        Console.WriteLine("===========================");

        var result = await _appointmentService.CancelAsync(id, GetUserId(), dto, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Patient,Admin")]
    [HttpPut("{id:int}/reschedule")]
    public async Task<IActionResult> Reschedule(
        int id, [FromBody] RescheduleAppointmentRequestDto dto, CancellationToken ct)
    {
        var result = await _appointmentService.RescheduleAsync(id, GetUserId(), dto, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Doctor")]
    [HttpPut("{id:int}/mark-urgent")]
    public async Task<IActionResult> MarkUrgent(
        int id, [FromBody] MarkUrgentRequestDto dto, CancellationToken ct)
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

    /// <summary>
    /// Mark attendance for a confirmed appointment.
    /// SRS §4.2 (Admin) and §4.3 (Doctor) — both roles can mark attendance.
    /// BR-37: validated inside AppointmentService that appointment date+time has passed.
    /// </summary>
    [Authorize(Roles = "Admin,Doctor,Owner")]
    [HttpPut("{id:int}/mark-attendance")]
    public async Task<IActionResult> MarkAttendance(
        int id, [FromBody] MarkAttendanceRequestDto dto, CancellationToken ct)
    {
        var result = await _appointmentService.MarkAttendanceAsync(id, dto, ct);
        return Ok(result);
    }
}
