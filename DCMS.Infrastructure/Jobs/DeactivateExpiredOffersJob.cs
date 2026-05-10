using DCMS.Application.Interfaces;
using DCMS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DCMS.Infrastructure.Jobs;

/// <summary>
/// Automatically deactivates OfferDiscount records whose EndDate has passed.
/// Runs daily via Hangfire.
/// </summary>
public class DeactivateExpiredOffersJob
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;
    private readonly ILogger<DeactivateExpiredOffersJob> _logger;

    public DeactivateExpiredOffersJob(
        IUnitOfWork uow,
        INotificationService notificationService,
        ILogger<DeactivateExpiredOffersJob> logger)
    {
        _uow = uow;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expired = await _uow.OfferDiscounts.FindAsync(
            o => o.IsActive && o.EndDate < today, ct);

        var count = 0;
        foreach (var offer in expired)
        {
            offer.IsActive = false;
            count++;
        }

        if (count > 0)
        {
            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation("DeactivateExpiredOffersJob: Deactivated {Count} expired offers", count);
        }
    }
}
