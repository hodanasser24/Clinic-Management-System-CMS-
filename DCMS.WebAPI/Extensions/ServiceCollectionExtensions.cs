using DCMS.Application.Interfaces;
using DCMS.Application.Mappings;
using DCMS.Application.Services;
using DCMS.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace DCMS.WebAPI.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Application-layer services exactly once.
    /// Previously these were registered both here AND in Program.cs, causing duplicate registrations.
    /// Program.cs now calls only this method — it no longer registers services manually.
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // FluentValidation — scan Application assembly for all validators
        services.AddValidatorsFromAssemblyContaining<AppointmentRequestValidator>();

        // ── Application Services ───────────────────────────────────────────────
        services.AddScoped<IAuthService,                AuthService>();
        services.AddScoped<IAppointmentService,         AppointmentService>();
        services.AddScoped<IReportService,              ReportService>();
        services.AddScoped<IPrescriptionService,        PrescriptionService>();
        services.AddScoped<IDentalChartService,         DentalChartService>();
        services.AddScoped<IScheduleService,            ScheduleService>();
        services.AddScoped<IModificationRequestService, ModificationRequestService>();
        services.AddScoped<INotificationService,        NotificationService>();
        services.AddScoped<IDashboardService,           DashboardService>();
        services.AddScoped<IOwnerService,               OwnerService>();
        services.AddScoped<ISystemLogService,           SystemLogService>();
        services.AddScoped<IProfileService,             ProfileService>();
        services.AddScoped<IRevenueService,             RevenueService>();
        services.AddScoped<IContactMessageService,      ContactMessageService>();

        // ASP.NET Core Identity PasswordHasher (used by AuthService and OwnerService)
        services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<DCMS.Domain.Entities.User>,
            Microsoft.AspNetCore.Identity.PasswordHasher<DCMS.Domain.Entities.User>>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key        = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken            = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = jwtSection["Issuer"],
                    ValidAudience            = jwtSection["Audience"],
                    IssuerSigningKey         = new SymmetricSecurityKey(key),
                    ClockSkew                = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "DCMS API",
                Version     = "v1",
                Description = "Dental Clinic Management System — Backend API"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name        = "Authorization",
                Type        = SecuritySchemeType.ApiKey,
                Scheme      = "Bearer",
                BearerFormat = "JWT",
                In          = ParameterLocation.Header,
                Description = "Enter: Bearer {your JWT token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
