using DCMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DCMS.Application.Interfaces;

/// <summary>
/// Application-layer contract for the EF Core DbContext.
/// Services depend on this interface – never on the concrete DbContext.
/// </summary>
public interface IAppDbContext
{
    // ── Identity / Users ─────────────────────────────────────────────────────
    DbSet<User>                      Users                   { get; }

    // ── Clinic Core ───────────────────────────────────────────────────────────
    DbSet<Branch>                    Branches                { get; }
    DbSet<Service>                   Services                { get; }
    DbSet<OfferDiscount>             OfferDiscounts          { get; }
    DbSet<FAQ>                       FAQs                    { get; }

    // ── Appointments ──────────────────────────────────────────────────────────
    DbSet<Appointment>               Appointments            { get; }

    // ── Schedules ─────────────────────────────────────────────────────────────
    DbSet<Schedule>                  Schedules               { get; }
    DbSet<ScheduleChangeRequest>     ScheduleChangeRequests  { get; }

    // ── Service Modification ──────────────────────────────────────────────────
    DbSet<ServiceModificationRequest> ServiceModificationRequests { get; }

    // ── Medical Records ───────────────────────────────────────────────────────
    DbSet<Report>                    Reports                 { get; }
    DbSet<ToothRecord>               ToothRecords            { get; }
    DbSet<Prescription>              Prescriptions           { get; }
    DbSet<PrescriptionItem>          PrescriptionItems       { get; }

    // ── Communication ─────────────────────────────────────────────────────────
    DbSet<Notification>              Notifications           { get; }
    DbSet<ContactMessage>            ContactMessages         { get; }

    // ── Logging ───────────────────────────────────────────────────────────────
    DbSet<SystemLog>                 SystemLogs              { get; }

    // ── Persistence ───────────────────────────────────────────────────────────
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
