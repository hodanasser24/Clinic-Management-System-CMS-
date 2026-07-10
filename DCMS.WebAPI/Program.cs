using DCMS.Infrastructure.DependencyInjection;
using DCMS.Infrastructure.Jobs;
using DCMS.WebAPI.Extensions;
using DCMS.WebAPI.Middleware;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.AspNetCore;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DCMS.Infrastructure.Data.ApplicationDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.IPasswordHasher<DCMS.Domain.Entities.User>>();

    // ── Ensure Owner exists with a known password ──────────────────────────
    var owner = db.Users.FirstOrDefault(u => u.Email == "owner@clinic.com");
    if (owner == null)
    {
        owner = new DCMS.Domain.Entities.Owner { FullName = "Owner", Email = "owner@clinic.com", Role = DCMS.Domain.Enums.UserRole.Owner, Phone = "0100", IsActive = true, Specialization = "Surgery", Qualification = "MD", ExperienceYears = 15 };
        owner.PasswordHash = hasher.HashPassword(owner, "Admin123!");
        db.Users.Add(owner);
    }
    else if (hasher.VerifyHashedPassword(owner, owner.PasswordHash, "Admin123!") == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
    {
        owner.PasswordHash = hasher.HashPassword(owner, "Admin123!");
    }

    // ── Ensure Admin exists (seed if missing, fix hash if wrong) ───────────
    var admin = db.Users.FirstOrDefault(u => u.Email == "admin@clinic.com");
    if (admin == null)
    {
        admin = new DCMS.Domain.Entities.Admin { FullName = "Admin User", Email = "admin@clinic.com", Role = DCMS.Domain.Enums.UserRole.Admin, Phone = "01000000001", IsActive = true };
        admin.PasswordHash = hasher.HashPassword(admin, "Password123!");
        db.Users.Add(admin);
    }
    else if (hasher.VerifyHashedPassword(admin, admin.PasswordHash, "Password123!") == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
    {
        admin.PasswordHash = hasher.HashPassword(admin, "Password123!");
    }

    // ── Ensure Doctor exists (seed if missing, fix hash if wrong) ──────────
    var doctor = db.Users.FirstOrDefault(u => u.Email == "doctor@clinic.com");
    if (doctor == null)
    {
        doctor = new DCMS.Domain.Entities.Doctor { FullName = "Doctor User", Email = "doctor@clinic.com", Role = DCMS.Domain.Enums.UserRole.Doctor, Phone = "01000000002", IsActive = true, Specialization = "Dentist", Qualification = "MD", ExperienceYears = 5 };
        doctor.PasswordHash = hasher.HashPassword(doctor, "Password123!");
        db.Users.Add(doctor);
    }
    else if (hasher.VerifyHashedPassword(doctor, doctor.PasswordHash, "Password123!") == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
    {
        doctor.PasswordHash = hasher.HashPassword(doctor, "Password123!");
    }

    db.SaveChanges();
}
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
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext == null) return false;

        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole("Owner");
    }
}
