using DCMS.Application.Interfaces;
using DCMS.Domain.Interfaces;
using DCMS.Domain.Interfaces.Repositories;
using DCMS.Infrastructure.Data;
using DCMS.Infrastructure.Identity;
using DCMS.Infrastructure.Jobs;
using DCMS.Infrastructure.Repositories;
using DCMS.Infrastructure.Services;
using DCMS.Infrastructure.UnitOfWork;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DCMS.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ── Database ───────────────────────────────────────────────────────────
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register as IAppDbContext for services that need raw DbContext access
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // ── Identity ───────────────────────────────────────────────────────────
        services.AddScoped(typeof(IPasswordHasher<>), typeof(PasswordHasher<>));
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // ── HTTP Context (for CurrentUserService and guest session in services) ─
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ── Specialized Repositories ───────────────────────────────────────────
        // These MUST be registered before UnitOfWork, which receives them via DI.
        // Missing registrations here were the root cause of the runtime DI crash.
        services.AddScoped<IPatientRepository,     PatientRepository>();
        services.AddScoped<IDoctorRepository,      DoctorRepository>();
        services.AddScoped<IAdminRepository,       AdminRepository>();
        services.AddScoped<IGuestRepository,       GuestRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IScheduleRepository,    ScheduleRepository>();
        services.AddScoped<IReportRepository,      ReportRepository>();

        // ── Unit of Work ───────────────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        // ── Email ──────────────────────────────────────────────────────────────
        services.AddScoped<IEmailService, EmailNotificationService>();

        // ── Hangfire ──────────────────────────────────────────────────────────
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(
                configuration.GetConnectionString("DefaultConnection"),
                new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout      = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout  = TimeSpan.FromMinutes(5),
                    QueuePollInterval           = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks          = true
                }));

        services.AddHangfireServer();

        // ── Background Jobs (Scoped so they can resolve IUnitOfWork) ──────────
        services.AddScoped<AppointmentReminderJob>();
        services.AddScoped<ExpireScheduleChangeRequestsJob>();
        services.AddScoped<DeactivateExpiredOffersJob>();
        services.AddScoped<CleanupSystemLogsJob>();

        return services;
    }
}
