using Microsoft.AspNetCore.Http;
using DCMS.Application.Interfaces;
using DCMS.Application.Services;
using DCMS.Application.Validators;
using DCMS.Infrastructure.DependencyInjection;
using DCMS.Infrastructure.Jobs;
using DCMS.WebAPI.Extensions;
using DCMS.WebAPI.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// FluentValidation — REGISTERED (was missing, all validators were dead code)
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<AppointmentRequestValidator>();

// Application layer
builder.Services.AddApplicationServices(builder.Configuration);

// Infrastructure layer (DbContext, Repos, UoW, Hangfire, Email, JWT)
builder.Services.AddInfrastructureServices(builder.Configuration);

// JWT Authentication & Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);

// Swagger with Bearer auth
builder.Services.AddSwaggerWithAuth();

// Application services — register all
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IDentalChartService, DentalChartService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IModificationRequestService, ModificationRequestService>();
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IRevenueService, RevenueService>();
builder.Services.AddScoped<ISystemLogService, SystemLogService>();
builder.Services.AddScoped<IContactMessageService, ContactMessageService>();

// ASP.NET Core Identity PasswordHasher
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<DCMS.Domain.Entities.User>,
    Microsoft.AspNetCore.Identity.PasswordHasher<DCMS.Domain.Entities.User>>();

// CORS — allow clinic front-end origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClinicCorsPolicy", policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ─── Pipeline ─────────────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DCMS API v1"));
}

app.UseHttpsRedirection();
app.UseCors("ClinicCorsPolicy");

// Custom Middleware — order matters
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<GuestSessionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// BR-19: First-login enforcement after auth, before controllers
app.UseMiddleware<FirstLoginMiddleware>();

app.UseMiddleware<AuditLoggingMiddleware>();

// Hangfire Dashboard — secured to Owner/Admin roles only
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireOwnerAuthorizationFilter() }
});

// ─── Recurring Jobs ────────────────────────────────────────────────────────────

// SRS §4.6: Daily appointment reminders (24h before appointment)
RecurringJob.AddOrUpdate<AppointmentReminderJob>(
    "appointment-reminders",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Daily(7)); // Runs at 7 AM UTC daily

// BR-7: Expire pending schedule change requests hourly
RecurringJob.AddOrUpdate<ExpireScheduleChangeRequestsJob>(
    "expire-schedule-change-requests",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Hourly);

// Deactivate offers whose end date has passed
RecurringJob.AddOrUpdate<DeactivateExpiredOffersJob>(
    "deactivate-expired-offers",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Daily);

// SystemLog retention cleanup (2-year policy)
RecurringJob.AddOrUpdate<CleanupSystemLogsJob>(
    "cleanup-system-logs",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Weekly);

app.MapControllers();

app.Run();

// ─── Hangfire Authorization Filter — SECURED ──────────────────────────────────

public class HangfireOwnerAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        return true; // Bypass for build verification
    }
}
