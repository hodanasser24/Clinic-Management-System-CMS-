using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces.Repositories;

namespace DCMS.Domain.Interfaces;

/// <summary>
/// Unit of Work contract.
///
/// FIX-5:  BeginTransactionAsync / CommitTransactionAsync / RollbackTransactionAsync
///         now accept CancellationToken so HTTP request cancellation propagates
///         all the way into open DB transactions — preventing connection leaks.
///
/// IMPROVE-2: All 22 repositories are exposed through this interface.
///            Application services must NEVER depend on the concrete UnitOfWork
///            or on ApplicationDbContext directly.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // ── User hierarchy ─────────────────────────────────────────────────────
    IPatientRepository  Patients  { get; }
    IDoctorRepository   Doctors   { get; }
    IAdminRepository    Admins    { get; }
    IGuestRepository    Guests    { get; }
    // Owner is accessed via Doctors (Owner : Doctor) — no separate repo needed
    // Owner-specific navigation collections are queried through IDoctorRepository.

    // ── Core clinic operations ─────────────────────────────────────────────
    IAppointmentRepository                          Appointments                    { get; }
    IScheduleRepository                             Schedules                       { get; }
    IReportRepository                               Reports                         { get; }
    IGenericRepository<Branch>                      Branches                        { get; }
    IGenericRepository<Service>                     Services                        { get; }
    IGenericRepository<FAQ>                         FAQs                            { get; }
    IGenericRepository<OfferDiscount>               OfferDiscounts                  { get; }
    IGenericRepository<ContactMessage>              ContactMessages                 { get; }
    IGenericRepository<Revenue>                     Revenues                        { get; }

    // ── Clinical module ────────────────────────────────────────────────────
    IGenericRepository<Prescription>                Prescriptions                   { get; }
    IGenericRepository<PrescriptionItem>            PrescriptionItems               { get; }
    IGenericRepository<DentalChart>                 DentalCharts                    { get; }
    IGenericRepository<ToothRecord>                 ToothRecords                    { get; }

    // ── Approval workflows ─────────────────────────────────────────────────
    IGenericRepository<ScheduleChangeRequest>       ScheduleChangeRequests          { get; }
    IGenericRepository<ServiceModificationRequest>  ServiceModificationRequests     { get; }
    IGenericRepository<FAQModificationRequest>      FAQModificationRequests         { get; }
    IGenericRepository<OfferDiscountModificationRequest> OfferDiscountModificationRequests { get; }
    IGenericRepository<BranchModificationRequest>   BranchModificationRequests      { get; }

    // ── Cross-cutting concerns ─────────────────────────────────────────────
    IGenericRepository<Notification>                Notifications                   { get; }
    IGenericRepository<SystemLog>                   SystemLogs                      { get; }

    // ── Persistence ────────────────────────────────────────────────────────
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>FIX-5: CancellationToken propagates into the underlying IDbContextTransaction.</summary>
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
