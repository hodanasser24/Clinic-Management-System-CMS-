using DCMS.Infrastructure.DependencyInjection;
using DCMS.Infrastructure.Jobs;
using DCMS.WebAPI.Extensions;
using DCMS.WebAPI.Middleware;
using FluentValidation.AspNetCore;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ────────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// ── FluentValidation auto-validation ──────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();

// ── Infrastructure layer (DbContext, Repos, UoW, Hangfire, Email, JWT service) ─
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── Application layer (all services registered exactly once) ───────────────────
builder.Services.AddApplicationServices(builder.Configuration);

// ── JWT Authentication & Authorization ────────────────────────────────────────
builder.Services.AddJwtAuthentication(builder.Configuration);

// ── Swagger with Bearer auth ───────────────────────────────────────────────────
builder.Services.AddSwaggerWithAuth();

// ── CORS ───────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClinicCorsPolicy", policy =>
    {
        policy
            .WithOrigins(
                builder.Configuration
                       .GetSection("AllowedOrigins")
                       .Get<string[]>() ?? Array.Empty<string>())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ── Build ──────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DCMS API v1"));
}

app.UseHttpsRedirection();
app.UseCors("ClinicCorsPolicy");

// ── Middleware pipeline (order matters) ───────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<GuestSessionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// BR-19: first-login enforcement runs after auth, before controllers
app.UseMiddleware<FirstLoginMiddleware>();
app.UseMiddleware<AuditLoggingMiddleware>();

// ── Hangfire dashboard (Owner only) ───────────────────────────────────────────
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireOwnerAuthorizationFilter() }
});

// ── Recurring background jobs ──────────────────────────────────────────────────
RecurringJob.AddOrUpdate<AppointmentReminderJob>(
    "appointment-reminders",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Daily(7));

RecurringJob.AddOrUpdate<ExpireScheduleChangeRequestsJob>(
    "expire-schedule-change-requests",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Hourly);

RecurringJob.AddOrUpdate<DeactivateExpiredOffersJob>(
    "deactivate-expired-offers",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Daily);

RecurringJob.AddOrUpdate<CleanupSystemLogsJob>(
    "cleanup-system-logs",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Weekly);

app.MapControllers();
app.Run();

// ── Hangfire dashboard authorization ──────────────────────────────────────────
public class HangfireOwnerAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole("Owner");
    }
}
