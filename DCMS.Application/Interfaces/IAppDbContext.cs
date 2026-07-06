using DCMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DCMS.Application.Interfaces;

/// <summary>
/// Application-layer abstraction for the EF Core DbContext.
/// Services depend only on this interface — never on the concrete ApplicationDbContext.
/// </summary>
public interface IAppDbContext
{
    // ── User hierarchy ────────────────────────────────────────────────────
    DbSet<User>    Users    { get; }
    DbSet<Patient> Patients { get; }
    DbSet<Doctor>  Doctors  { get; }
    DbSet<Admin>   Admins   { get; }
    DbSet<Guest>   Guests   { get; }

    // ── Clinic core ───────────────────────────────────────────────────────
    DbSet<Branch>       Branches      { get; }
    DbSet<Service>      Services      { get; }
    DbSet<OfferDiscount> OfferDiscounts { get; }
    DbSet<FAQ>          FAQs          { get; }
    DbSet<Revenue>      Revenues      { get; }

    // ── Appointments & schedules ──────────────────────────────────────────
    DbSet<Appointment>          Appointments          { get; }
    DbSet<Schedule>             Schedules             { get; }
    DbSet<ScheduleChangeRequest> ScheduleChangeRequests { get; }

    // ── Modification requests ─────────────────────────────────────────────
    DbSet<ServiceModificationRequest>        ServiceModificationRequests       { get; }
    DbSet<FAQModificationRequest>            FAQModificationRequests           { get; }
    DbSet<OfferDiscountModificationRequest>  OfferDiscountModificationRequests { get; }
    DbSet<BranchModificationRequest>         BranchModificationRequests        { get; }

    // ── Medical records ───────────────────────────────────────────────────
    DbSet<Report>          Reports          { get; }
    DbSet<Prescription>    Prescriptions    { get; }
    DbSet<PrescriptionItem> PrescriptionItems { get; }
    DbSet<DentalChart>     DentalCharts     { get; }
    DbSet<ToothRecord>     ToothRecords     { get; }

    // ── Communication & logging ───────────────────────────────────────────
    DbSet<Notification>   Notifications   { get; }
    DbSet<ContactMessage> ContactMessages { get; }
    DbSet<SystemLog>      SystemLogs      { get; }

    // ── Persistence ────────────────────────────────────────────────────────
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
