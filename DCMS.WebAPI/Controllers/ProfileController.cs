using DCMS.Application.DTOs.Profile;
using DCMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DCMS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService) => _profileService = profileService;

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── Patient profile ───────────────────────────────────────────────────────

    [Authorize(Roles = "Patient")]
    [HttpGet("patient")]
    public async Task<IActionResult> GetPatientProfile(CancellationToken ct)
    {
        var result = await _profileService.GetPatientProfileAsync(GetUserId(), ct);
        return Ok(result);
    }

    [Authorize(Roles = "Patient")]
    [HttpPut("patient")]
    public async Task<IActionResult> UpdatePatientProfile([FromBody] UpdatePatientProfileRequestDto dto, CancellationToken ct)
    {
        var result = await _profileService.UpdatePatientProfileAsync(GetUserId(), dto, ct);
        return Ok(result);
    }

    // ── Doctor profile (self-update) ──────────────────────────────────────────

    [Authorize(Roles = "Doctor,Owner")]
    [HttpGet("doctor")]
    public async Task<IActionResult> GetDoctorProfile(CancellationToken ct)
    {
        var result = await _profileService.GetDoctorProfileAsync(GetUserId(), ct);
        return Ok(result);
    }

    [Authorize(Roles = "Doctor,Owner")]
    [HttpPut("doctor")]
    public async Task<IActionResult> UpdateDoctorProfile([FromBody] UpdateDoctorSelfProfileRequestDto dto, CancellationToken ct)
    {
        var result = await _profileService.UpdateDoctorSelfProfileAsync(GetUserId(), dto, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Doctor,Owner")]
    [HttpPut("doctor/photo")]
    public async Task<IActionResult> UpdateDoctorPhoto([FromBody] UpdatePhotoRequestDto dto, CancellationToken ct)
    {
        var result = await _profileService.UpdateDoctorPhotoAsync(GetUserId(), dto, ct);
        return Ok(result);
    }
}
