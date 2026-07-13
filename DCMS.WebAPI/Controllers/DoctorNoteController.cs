using DCMS.Application.DTOs.DoctorNote;
using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DCMS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Doctor,Admin,Owner")]
public class DoctorNoteController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public DoctorNoteController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("sub");
        return int.TryParse(claim, out var id) ? id : null;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotes(CancellationToken ct)
    {
        var doctorId = GetUserId();
        if (doctorId == null) return Unauthorized();

        // Since we are not using a service, we fetch entities and map manually
        var notes = await _uow.DoctorNotes.FindAsync(n => n.DoctorId == doctorId.Value, ct);
        
        var dtos = notes.OrderByDescending(n => n.CreatedAt).Select(n => new DoctorNoteDto
        {
            Id = n.Id,
            PatientId = n.PatientId,
            Content = n.Content,
            CreatedAt = n.CreatedAt
        });

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDoctorNoteDto dto, CancellationToken ct)
    {
        var doctorId = GetUserId();
        if (doctorId == null) return Unauthorized();
        
        var note = new DoctorNote
        {
            DoctorId = doctorId.Value,
            Content = dto.Content
        };

        await _uow.DoctorNotes.AddAsync(note, ct);
        await _uow.SaveChangesAsync(ct);

        var resultDto = new DoctorNoteDto
        {
            Id = note.Id,
            PatientId = note.PatientId,
            Content = note.Content,
            CreatedAt = note.CreatedAt
        };

        return CreatedAtAction(nameof(GetMyNotes), new { id = note.Id }, resultDto);
    }

    [Authorize(Roles = "Doctor,Owner")]
    [HttpGet("by-patient/{patientId:int}")]
    public async Task<IActionResult> GetPatientNotes(int patientId, CancellationToken ct)
    {
        var doctorId = GetUserId();
        if (doctorId == null) return Unauthorized();

        var notes = await _uow.DoctorNotes.FindAsync(
            note => note.DoctorId == doctorId.Value && note.PatientId == patientId, ct);

        return Ok(notes.OrderByDescending(note => note.CreatedAt).Select(note => new DoctorNoteDto
        {
            Id = note.Id,
            PatientId = note.PatientId,
            Content = note.Content,
            CreatedAt = note.CreatedAt
        }));
    }

    [Authorize(Roles = "Doctor,Owner")]
    [HttpPost("by-patient/{patientId:int}")]
    public async Task<IActionResult> CreatePatientNote(
        int patientId, CreateDoctorNoteDto dto, CancellationToken ct)
    {
        var doctorId = GetUserId();
        if (doctorId == null) return Unauthorized();

        var patient = await _uow.Patients.GetByIdAsync(patientId, ct);
        if (patient == null) return NotFound(new { message = "Patient not found." });

        var note = new DoctorNote { DoctorId = doctorId.Value, PatientId = patientId, Content = dto.Content };
        await _uow.DoctorNotes.AddAsync(note, ct);
        await _uow.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetPatientNotes), new { patientId }, new DoctorNoteDto
        {
            Id = note.Id,
            PatientId = note.PatientId,
            Content = note.Content,
            CreatedAt = note.CreatedAt
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var doctorId = GetUserId();
        if (doctorId == null) return Unauthorized();

        var note = await _uow.DoctorNotes.GetByIdAsync(id, ct);

        if (note == null)
            return NotFound();

        // Owner check
        if (note.DoctorId != doctorId)
            return Forbid();

        _uow.DoctorNotes.Remove(note);
        await _uow.SaveChangesAsync(ct);

        return NoContent();
    }
}
