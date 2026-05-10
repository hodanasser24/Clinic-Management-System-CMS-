using DCMS.Application.Interfaces;
using DCMS.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.WebAPI.Controllers;

/// <summary>
/// SRS §4.1, §4.5 — Public read-only endpoints for browsing clinic info.
/// Accessible by Guests and unauthenticated users.
/// </summary>
[ApiController]
[Route("api/public")]
[AllowAnonymous]
public class PublicController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IProfileService _profileService;

    public PublicController(IUnitOfWork uow, IProfileService profileService)
    {
        _uow = uow;
        _profileService = profileService;
    }

    /// <summary>List all active branches.</summary>
    [HttpGet("branches")]
    public async Task<IActionResult> GetBranches(CancellationToken ct)
    {
        var branches = await _uow.Branches.FindAsync(b => b.IsActive, ct);
        var result = branches.Select(b => new
        {
            b.Id,
            b.Name,
            b.Location,
            b.Phone,
            b.WorkingHours
        });
        return Ok(result);
    }

    /// <summary>List all active services with pricing.</summary>
    [HttpGet("services")]
    public async Task<IActionResult> GetServices(CancellationToken ct)
    {
        var services = await _uow.Services.FindAsync(s => s.IsActive, ct);
        var result = services.Select(s => new
        {
            s.Id,
            s.Name,
            s.Description,
            s.Price,
            s.EstimatedDurationMinutes
        });
        return Ok(result);
    }

    /// <summary>List all active FAQs ordered by display order.</summary>
    [HttpGet("faqs")]
    public async Task<IActionResult> GetFAQs(CancellationToken ct)
    {
        var faqs = await _uow.FAQs.FindAsync(f => f.IsActive, ct);
        var result = faqs
            .OrderBy(f => f.DisplayOrder)
            .Select(f => new { f.Id, f.Question, f.Answer, f.DisplayOrder });
        return Ok(result);
    }

    /// <summary>List active doctors (public info only — no email/phone).</summary>
    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors(CancellationToken ct)
    {
        var doctors = await _uow.Doctors.FindAsync(d => d.IsActive, ct);
        var result = doctors
            .Where(d => d.Role == DCMS.Domain.Enums.UserRole.Doctor)
            .Select(d => new
            {
                d.Id,
                d.FullName,
                d.Specialization,
                d.Qualification,
                d.Bio,
                d.PhotoUrl,
                d.ExperienceYears
            });
        return Ok(result);
    }

    /// <summary>List currently active offers/discounts.</summary>
    [HttpGet("offers")]
    public async Task<IActionResult> GetActiveOffers(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var offers = await _uow.OfferDiscounts.FindAsync(
            o => o.IsActive && o.StartDate <= today && o.EndDate >= today, ct);
        var result = offers.Select(o => new
        {
            o.Id,
            o.Title,
            o.Description,
            o.DiscountPercentage,
            o.StartDate,
            o.EndDate,
            BranchName = o.Branch?.Name,
            ServiceName = o.Service?.Name
        });
        return Ok(result);
    }

    /// <summary>Get available appointment slots for a doctor on a specific date.</summary>
    [HttpGet("doctors/{doctorId:int}/available-slots")]
    public async Task<IActionResult> GetAvailableSlots(
        int doctorId,
        [FromQuery] int branchId,
        [FromQuery] DateOnly date,
        [FromServices] IScheduleService scheduleService,
        CancellationToken ct)
    {
        var schedule = await _uow.Schedules.GetByDoctorBranchDayAsync(doctorId, branchId, date.DayOfWeek, ct);
        if (schedule == null) return Ok(Array.Empty<object>());
        var slots = await scheduleService.GetAvailableTimeSlotsAsync(schedule.Id, date, ct);
        return Ok(slots);
    }
}
