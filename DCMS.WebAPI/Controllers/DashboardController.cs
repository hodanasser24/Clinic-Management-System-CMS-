using DCMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.WebAPI.Controllers;

/// <summary>
/// SRS §4.2 (Admin) and §4.4 (Owner): both roles require dashboard access.
/// Previous implementation had [Authorize(Roles = "Admin")] which blocked Owner — now fixed.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Owner")]   // FIX: Owner was previously blocked
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>High-level clinic summary: appointment counts, revenue, pending items.</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
    {
        var result = await _dashboardService.GetSummaryAsync(ct);
        return Ok(result);
    }

    /// <summary>Detailed breakdown for a specific calendar date.</summary>
    [HttpGet("daily")]
    public async Task<IActionResult> DailyReport([FromQuery] DateOnly date, CancellationToken ct)
    {
        var result = await _dashboardService.GetDailyReportAsync(date, ct);
        return Ok(result);
    }

    /// <summary>Week-long breakdown starting from weekStart (Monday recommended).</summary>
    [HttpGet("weekly")]
    public async Task<IActionResult> WeeklyReport(
        [FromQuery] DateOnly weekStart, CancellationToken ct)
    {
        var result = await _dashboardService.GetWeeklyReportAsync(weekStart, ct);
        return Ok(result);
    }

    /// <summary>Download daily report as CSV.</summary>
    [HttpGet("daily/export")]
    public async Task<IActionResult> ExportDaily([FromQuery] DateOnly date, CancellationToken ct)
    {
        var bytes = await _dashboardService.ExportDailyReportAsCsvAsync(date, ct);
        return File(bytes, "text/csv", $"daily-report-{date:yyyy-MM-dd}.csv");
    }

    /// <summary>Download weekly report as CSV.</summary>
    [HttpGet("weekly/export")]
    public async Task<IActionResult> ExportWeekly(
        [FromQuery] DateOnly weekStart, CancellationToken ct)
    {
        var bytes = await _dashboardService.ExportWeeklyReportAsCsvAsync(weekStart, ct);
        return File(bytes, "text/csv", $"weekly-report-{weekStart:yyyy-MM-dd}.csv");
    }
}
