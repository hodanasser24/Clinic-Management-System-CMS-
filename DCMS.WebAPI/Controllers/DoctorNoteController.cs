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

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetMyNotes(CancellationToken ct)
    {
        var doctorId = GetUserId();
        // Since we are not using a service, we fetch entities and map manually
        var notes = await _uow.DoctorNotes.FindAsync(n => n.DoctorId == doctorId, ct);
        
        var dtos = notes.OrderByDescending(n => n.CreatedAt).Select(n => new DoctorNoteDto
        {
            Id = n.Id,
            Content = n.Content,
            CreatedAt = n.CreatedAt
        });

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDoctorNoteDto dto, CancellationToken ct)
    {
        var doctorId = GetUserId();
        
        var note = new DoctorNote
        {
            DoctorId = doctorId,
            Content = dto.Content
        };

        await _uow.DoctorNotes.AddAsync(note, ct);
        await _uow.SaveChangesAsync(ct);

        var resultDto = new DoctorNoteDto
        {
            Id = note.Id,
            Content = note.Content,
            CreatedAt = note.CreatedAt
        };

        return CreatedAtAction(nameof(GetMyNotes), new { id = note.Id }, resultDto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var doctorId = GetUserId();
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
