using DCMS.Application.DTOs.ModificationRequests;
using DCMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DCMS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FAQController : ControllerBase
{
    private readonly IModificationRequestService _modificationRequestService;

    public FAQController(IModificationRequestService modificationRequestService)
    {
        _modificationRequestService = modificationRequestService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _modificationRequestService.GetFAQRequestsAsync(page, pageSize, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFAQModificationRequestDto dto, CancellationToken ct)
    {
        dto.AdminId = GetUserId();
        var result = await _modificationRequestService.CreateFAQRequestAsync(dto, ct);
        return CreatedAtAction(nameof(GetAll), result);
    }
}
