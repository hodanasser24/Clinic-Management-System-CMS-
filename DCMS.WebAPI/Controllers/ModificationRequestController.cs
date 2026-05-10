using DCMS.Application.DTOs.ModificationRequests;
using DCMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DCMS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ModificationRequestController : ControllerBase
{
    private readonly IModificationRequestService _modificationRequestService;

    public ModificationRequestController(IModificationRequestService modificationRequestService)
    {
        _modificationRequestService = modificationRequestService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // --- Service Modification Requests ---

    [Authorize(Roles = "Admin")]
    [HttpPost("services")]
    public async Task<IActionResult> CreateServiceRequest([FromBody] CreateServiceModificationRequestDto dto, CancellationToken ct)
    {
        dto.AdminId = GetUserId();
        var result = await _modificationRequestService.CreateServiceRequestAsync(dto, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpGet("services")]
    public async Task<IActionResult> GetServiceRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _modificationRequestService.GetServiceRequestsAsync(page, pageSize, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("services/{id:int}/decision")]
    public async Task<IActionResult> DecideServiceRequest(int id, [FromBody] ApproveRejectModificationRequestDto dto, CancellationToken ct)
    {
        var ownerId = GetUserId();
        var result = await _modificationRequestService.ApproveRejectServiceRequestAsync(id, dto, ownerId, ct);
        return Ok(result);
    }

    // --- FAQ Modification Requests ---

    [Authorize(Roles = "Admin")]
    [HttpPost("faqs")]
    public async Task<IActionResult> CreateFAQRequest([FromBody] CreateFAQModificationRequestDto dto, CancellationToken ct)
    {
        dto.AdminId = GetUserId();
        var result = await _modificationRequestService.CreateFAQRequestAsync(dto, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpGet("faqs")]
    public async Task<IActionResult> GetFAQRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _modificationRequestService.GetFAQRequestsAsync(page, pageSize, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("faqs/{id:int}/decision")]
    public async Task<IActionResult> DecideFAQRequest(int id, [FromBody] ApproveRejectModificationRequestDto dto, CancellationToken ct)
    {
        var ownerId = GetUserId();
        var result = await _modificationRequestService.ApproveRejectFAQRequestAsync(id, dto, ownerId, ct);
        return Ok(result);
    }

    // --- Offer/Discount Modification Requests ---

    [Authorize(Roles = "Admin")]
    [HttpPost("offers")]
    public async Task<IActionResult> CreateOfferRequest([FromBody] CreateOfferDiscountModificationRequestDto dto, CancellationToken ct)
    {
        dto.AdminId = GetUserId();
        var result = await _modificationRequestService.CreateOfferRequestAsync(dto, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpGet("offers")]
    public async Task<IActionResult> GetOfferRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _modificationRequestService.GetOfferRequestsAsync(page, pageSize, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("offers/{id:int}/decision")]
    public async Task<IActionResult> DecideOfferRequest(int id, [FromBody] ApproveRejectModificationRequestDto dto, CancellationToken ct)
    {
        var ownerId = GetUserId();
        var result = await _modificationRequestService.ApproveRejectOfferRequestAsync(id, dto, ownerId, ct);
        return Ok(result);
    }

    // --- Branch Modification Requests ---

    [Authorize(Roles = "Admin")]
    [HttpPost("branches")]
    public async Task<IActionResult> CreateBranchRequest([FromBody] CreateBranchModificationRequestDto dto, CancellationToken ct)
    {
        dto.AdminId = GetUserId();
        var result = await _modificationRequestService.CreateBranchRequestAsync(dto, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpGet("branches")]
    public async Task<IActionResult> GetBranchRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _modificationRequestService.GetBranchRequestsAsync(page, pageSize, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("branches/{id:int}/decision")]
    public async Task<IActionResult> DecideBranchRequest(int id, [FromBody] ApproveRejectModificationRequestDto dto, CancellationToken ct)
    {
        var ownerId = GetUserId();
        var result = await _modificationRequestService.ApproveRejectBranchRequestAsync(id, dto, ownerId, ct);
        return Ok(result);
    }
}
