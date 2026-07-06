using DCMS.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace DCMS.Infrastructure.Services;

public class EmailNotificationService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(IConfiguration configuration, ILogger<EmailNotificationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        var smtpHost = _configuration["Email:SmtpHost"]!;
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUser = _configuration["Email:SmtpUser"]!;
        var smtpPass = _configuration["Email:SmtpPass"]!;
        var fromEmail = _configuration["Email:FromEmail"]!;
        var fromName = _configuration["Email:FromName"] ?? "DCMS";

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        mailMessage.To.Add(new MailAddress(toEmail, toName));

        try
        {
            await client.SendMailAsync(mailMessage, ct);
            _logger.LogInformation("Email sent to {Email} — Subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendAppointmentConfirmationAsync(string toEmail, string toName, string doctorName, string branchName, string serviceName, DateOnly date, TimeOnly startTime, CancellationToken ct = default)
    {
        var subject = "Appointment Confirmed — DCMS";
        var body = $@"
            <h2>Appointment Confirmed</h2>
            <p>Dear {toName},</p>
            <p>Your appointment has been confirmed with the following details:</p>
            <ul>
                <li><strong>Doctor:</strong> {doctorName}</li>
                <li><strong>Branch:</strong> {branchName}</li>
                <li><strong>Service:</strong> {serviceName}</li>
                <li><strong>Date:</strong> {date:yyyy-MM-dd}</li>
                <li><strong>Time:</strong> {startTime:HH:mm}</li>
            </ul>
            <p>Thank you for choosing DCMS.</p>";

        await SendAsync(toEmail, toName, subject, body, ct);
    }

    public async Task SendAppointmentReminderAsync(string toEmail, string toName, string doctorName, string branchName, DateOnly date, TimeOnly startTime, CancellationToken ct = default)
    {
        var subject = "Appointment Reminder — DCMS";
        var body = $@"
            <h2>Appointment Reminder</h2>
            <p>Dear {toName},</p>
            <p>This is a reminder for your upcoming appointment:</p>
            <ul>
                <li><strong>Doctor:</strong> {doctorName}</li>
                <li><strong>Branch:</strong> {branchName}</li>
                <li><strong>Date:</strong> {date:yyyy-MM-dd}</li>
                <li><strong>Time:</strong> {startTime:HH:mm}</li>
            </ul>
            <p>We look forward to seeing you.</p>";

        await SendAsync(toEmail, toName, subject, body, ct);
    }

    public async Task SendPasswordResetAsync(string toEmail, string toName, string resetToken, CancellationToken ct = default)
    {
        var subject = "Password Reset — DCMS";
        var body = $@"
            <h2>Password Reset</h2>
            <p>Dear {toName},</p>
            <p>Your temporary password is: <strong>{resetToken}</strong></p>
            <p>Please log in and change your password immediately.</p>";

        await SendAsync(toEmail, toName, subject, body, ct);
    }
}
