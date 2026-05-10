using DCMS.Application.Interfaces;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DCMS.Infrastructure.Jobs;

/// <summary>
/// SRS §3.5, §4.6 — Send appointment reminder emails and notifications
/// to patients 24 hours before their confirmed appointments.
/// Runs daily via Hangfire.
/// </summary>
public class AppointmentReminderJob
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AppointmentReminderJob> _logger;

    public AppointmentReminderJob(
        IUnitOfWork uow,
        IEmailService emailService,
        INotificationService notificationService,
        ILogger<AppointmentReminderJob> logger)
    {
        _uow = uow;
        _emailService = emailService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var appointments = await _uow.Appointments.GetByDateAsync(tomorrow, ct);

        var confirmed = appointments
            .Where(a => a.Status == DCMS.Domain.Enums.AppointmentStatus.Confirmed)
            .ToList();

        _logger.LogInformation("AppointmentReminderJob: Sending reminders for {Count} appointments on {Date}",
            confirmed.Count, tomorrow);

        foreach (var appointment in confirmed)
        {
            try
            {
                // In-app notification
                await _notificationService.SendAsync(
                    appointment.PatientId,
                    NotificationType.AppointmentReminder,
                    NotificationPriority.High,
                    "Appointment Reminder",
                    $"Reminder: You have an appointment tomorrow ({tomorrow:yyyy-MM-dd}) at {appointment.StartTime}.",
                    appointment.Id, "Appointment", ct);

                // Email reminder if patient has email
                if (appointment.Patient != null)
                {
                    await _emailService.SendAppointmentReminderAsync(
                        appointment.Patient.Email,
                        appointment.Patient.FullName,
                        appointment.Doctor?.FullName ?? "Your doctor",
                        appointment.Branch?.Name ?? "the clinic",
                        tomorrow,
                        appointment.StartTime,
                        ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send reminder for appointment {Id}", appointment.Id);
            }
        }
    }
}
