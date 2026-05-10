using DCMS.Application.DTOs.SystemLogs;
using DCMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DCMS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SystemLogController : ControllerBase
{
    private readonly ISystemLogService _systemLogService;

    public SystemLogController(ISystemLogService systemLogService)
    {
        _systemLogService = systemLogService;
    }

    private string GetUserRole() => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    /// <summary>
    /// Owner: full audit log access with optional filters.
    /// Admin: restricted to schedule change logs only.
    /// SRS §6.5, §4.2, §4.4
    /// </summary>
    [Authorize(Roles = "Owner,Admin")]
    [HttpGet]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? entityType,
        [FromQuery] string? actionType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var role = GetUserRole();

        if (role == "Admin")
        {
            // Admin: schedule logs only
            var adminResult = await _systemLogService.GetScheduleLogsAsync(page, pageSize, ct);
            return Ok(adminResult);
        }

        // Owner: full access
        var filter = new SystemLogFilterDto
        {
            EntityType = entityType,
            ActionType = actionType,
            From = from,
            To = to,
            UserId = userId
        };
        var result = await _systemLogService.GetAllAsync(filter, page, pageSize, ct);
        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _systemLogService.GetByIdAsync(id, ct);
        return Ok(result);
    }
}
