namespace DCMS.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default);
    Task SendAppointmentConfirmationAsync(string toEmail, string toName, string doctorName, string branchName, string serviceName, DateOnly date, TimeOnly startTime, CancellationToken ct = default);
    Task SendAppointmentReminderAsync(string toEmail, string toName, string doctorName, string branchName, DateOnly date, TimeOnly startTime, CancellationToken ct = default);
    Task SendPasswordResetAsync(string toEmail, string toName, string resetToken, CancellationToken ct = default);
}
