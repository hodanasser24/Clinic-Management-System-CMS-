using DCMS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DCMS.Infrastructure.Jobs;

/// <summary>
/// Removes SystemLog records whose RetentionDate has passed (2-year retention policy).
/// Runs weekly via Hangfire.
/// </summary>
public class CleanupSystemLogsJob
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CleanupSystemLogsJob> _logger;

    public CleanupSystemLogsJob(IUnitOfWork uow, ILogger<CleanupSystemLogsJob> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var expired = await _uow.SystemLogs.FindAsync(l => l.RetentionDate < now, ct);
        var expiredList = expired.ToList();

        foreach (var log in expiredList)
            _uow.SystemLogs.Remove(log);

        if (expiredList.Count > 0)
        {
            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation("CleanupSystemLogsJob: Deleted {Count} expired log records", expiredList.Count);
        }
    }
}
