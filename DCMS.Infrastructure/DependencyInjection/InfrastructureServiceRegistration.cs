using DCMS.Application.Interfaces;
using DCMS.Domain.Interfaces;
using DCMS.Infrastructure.Data;
using DCMS.Infrastructure.Identity;
using DCMS.Infrastructure.Jobs;
using DCMS.Infrastructure.Repositories;
using DCMS.Infrastructure.Services;
using DCMS.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DCMS.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // ─── Database ─────────────────────────────────────────
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // ─── Identity ─────────────────────────────────────────
        services.AddScoped(typeof(IPasswordHasher<>), typeof(PasswordHasher<>));
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // ─── Current User ─────────────────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ─── DbContext as IAppDbContext ───────────────────────
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // ─── Unit of Work ─────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        // ─── Email ────────────────────────────────────────────
        services.AddScoped<IEmailService, EmailNotificationService>();

        // ─── Hangfire ─────────────────────────────────────────
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"),
                new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

        services.AddHangfireServer();

        // ─── Background Jobs (Scoped) ─────────────────────────
        services.AddScoped<AppointmentReminderJob>();
        services.AddScoped<ExpireScheduleChangeRequestsJob>();
        services.AddScoped<DeactivateExpiredOffersJob>();
        services.AddScoped<CleanupSystemLogsJob>();

        return services;
    }

    public static void ConfigureHangfireJobs(IServiceProvider serviceProvider)
    {
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
    }
}
