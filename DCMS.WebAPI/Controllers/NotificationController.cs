using DCMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly ICurrentUserService  _currentUser;

    public NotificationController(INotificationService service, ICurrentUserService currentUser)
    {
        _service     = service;
        _currentUser = currentUser;
    }

    /// <summary>
    /// GET /api/notification?page=1&pageSize=20&unreadOnly=false
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMine(
        [FromQuery] int  page       = 1,
        [FromQuery] int  pageSize   = 20,
        [FromQuery] bool unreadOnly = false,
        CancellationToken ct = default)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var result = await _service.GetByUserAsync(userId, page, pageSize, unreadOnly, ct);
        return Ok(result);
    }

    /// <summary>
    /// PATCH /api/notification/{id}/read
    /// </summary>
    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        await _service.MarkAsReadAsync(id, userId, ct);
        return NoContent();
    }

    /// <summary>
    /// PATCH /api/notification/read-all
    /// </summary>
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        await _service.MarkAllAsReadAsync(userId, ct);
        return NoContent();
    }
}
