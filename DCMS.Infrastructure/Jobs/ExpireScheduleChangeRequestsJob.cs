using DCMS.Application.Interfaces;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DCMS.Infrastructure.Jobs;

/// <summary>
/// BR-7: Expires ScheduleChangeRequests that have passed their ExpiresAt (24h) without full approval.
/// Runs hourly via Hangfire.
/// </summary>
public class ExpireScheduleChangeRequestsJob
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ExpireScheduleChangeRequestsJob> _logger;

    public ExpireScheduleChangeRequestsJob(
        IUnitOfWork uow,
        INotificationService notificationService,
        ILogger<ExpireScheduleChangeRequestsJob> logger)
    {
        _uow = uow;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var expired = await _uow.ScheduleChangeRequests.FindAsync(
            r => r.Status == RequestStatus.Pending && r.ExpiresAt < now, ct);

        var count = 0;
        foreach (var request in expired)
        {
            request.Status = RequestStatus.Expired;
            count++;

            // Notify doctor
            await _notificationService.SendAsync(
                request.RequestingDoctorId,
                NotificationType.ScheduleChangeExpired,
                NotificationPriority.Normal,
                "Schedule Change Request Expired",
                "Your schedule change request has expired without full approval.",
                request.Id, "ScheduleChangeRequest", ct);

            // Notify admin
            await _notificationService.SendToRoleAsync(
                UserRole.Admin, NotificationType.ScheduleChangeExpired, NotificationPriority.Normal,
                "Schedule Change Request Expired",
                $"Schedule change request #{request.Id} has expired.",
                request.Id, "ScheduleChangeRequest", ct);
        }

        if (count > 0)
        {
            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation("ExpireScheduleChangeRequestsJob: Expired {Count} pending requests", count);
        }
    }
}
