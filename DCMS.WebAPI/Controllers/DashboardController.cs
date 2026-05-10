using DCMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
    {
        var result = await _dashboardService.GetSummaryAsync(ct);
        return Ok(result);
    }

    [HttpGet("daily")]
    public async Task<IActionResult> DailyReport([FromQuery] DateOnly date, CancellationToken ct)
    {
        var result = await _dashboardService.GetDailyReportAsync(date, ct);
        return Ok(result);
    }

    [HttpGet("weekly")]
    public async Task<IActionResult> WeeklyReport([FromQuery] DateOnly weekStart, CancellationToken ct)
    {
        var result = await _dashboardService.GetWeeklyReportAsync(weekStart, ct);
        return Ok(result);
    }

    [HttpGet("daily/export")]
    public async Task<IActionResult> ExportDaily([FromQuery] DateOnly date, CancellationToken ct)
    {
        var bytes = await _dashboardService.ExportDailyReportAsCsvAsync(date, ct);
        return File(bytes, "text/csv", $"daily-report-{date:yyyy-MM-dd}.csv");
    }

    [HttpGet("weekly/export")]
    public async Task<IActionResult> ExportWeekly([FromQuery] DateOnly weekStart, CancellationToken ct)
    {
        var bytes = await _dashboardService.ExportWeeklyReportAsCsvAsync(weekStart, ct);
        return File(bytes, "text/csv", $"weekly-report-{weekStart:yyyy-MM-dd}.csv");
    }
}
