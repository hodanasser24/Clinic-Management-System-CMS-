using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces;
using DCMS.Domain.Interfaces.Repositories;
using DCMS.Infrastructure.Data;
using DCMS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace DCMS.Infrastructure.UnitOfWork;

/// <summary>
/// Concrete Unit of Work implementation.
///
/// FIX-5: All three transaction methods accept CancellationToken.
///        Forwarded to BeginTransactionAsync so client disconnects abort open
///        transactions and release DB connections immediately.
///
/// IMPROVE-2: All 22 repositories wired. Specialized repos (Patients, Doctors,
///            Admins, Guests, Appointments, Schedules, Reports) are injected via DI.
///            Generic repos are lazily initialized via ??= to avoid allocating
///            repositories that are never used in a given request.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    // ── Injected specialized repositories ─────────────────────────────────
    private readonly IPatientRepository     _patients;
    private readonly IDoctorRepository      _doctors;
    private readonly IAdminRepository       _admins;
    private readonly IGuestRepository       _guests;
    private readonly IAppointmentRepository _appointments;
    private readonly IScheduleRepository    _schedules;
    private readonly IReportRepository      _reports;

    // ── Generic repositories — lazily initialized ──────────────────────────
    private IGenericRepository<Branch>?                         _branches;
    private IGenericRepository<Service>?                        _services;
    private IGenericRepository<FAQ>?                            _faqs;
    private IGenericRepository<OfferDiscount>?                  _offerDiscounts;
    private IGenericRepository<ContactMessage>?                 _contactMessages;
    private IGenericRepository<Prescription>?                   _prescriptions;
    private IGenericRepository<PrescriptionItem>?               _prescriptionItems;
    private IGenericRepository<DentalChart>?                    _dentalCharts;
    private IGenericRepository<ToothRecord>?                    _toothRecords;
    private IGenericRepository<ScheduleChangeRequest>?          _scheduleChangeRequests;
    private IGenericRepository<ServiceModificationRequest>?     _serviceModRequests;
    private IGenericRepository<FAQModificationRequest>?         _faqModRequests;
    private IGenericRepository<OfferDiscountModificationRequest>? _offerModRequests;
    private IGenericRepository<BranchModificationRequest>?      _branchModRequests;
    private IGenericRepository<Notification>?                   _notifications;
    private IGenericRepository<SystemLog>?                      _systemLogs;

    public UnitOfWork(
        ApplicationDbContext context,
        IPatientRepository     patients,
        IDoctorRepository      doctors,
        IAdminRepository       admins,
        IGuestRepository       guests,
        IAppointmentRepository appointments,
        IScheduleRepository    schedules,
        IReportRepository      reports)
    {
        _context      = context;
        _patients     = patients;
        _doctors      = doctors;
        _admins       = admins;
        _guests       = guests;
        _appointments = appointments;
        _schedules    = schedules;
        _reports      = reports;
    }

    // ── IUnitOfWork properties ────────────────────────────────────────────
    public IPatientRepository     Patients     => _patients;
    public IDoctorRepository      Doctors      => _doctors;
    public IAdminRepository       Admins       => _admins;
    public IGuestRepository       Guests       => _guests;
    public IAppointmentRepository Appointments => _appointments;
    public IScheduleRepository    Schedules    => _schedules;
    public IReportRepository      Reports      => _reports;

    public IGenericRepository<Branch>           Branches        => _branches        ??= new GenericRepository<Branch>(_context);
    public IGenericRepository<Service>          Services        => _services        ??= new GenericRepository<Service>(_context);
    public IGenericRepository<FAQ>              FAQs            => _faqs            ??= new GenericRepository<FAQ>(_context);
    public IGenericRepository<OfferDiscount>    OfferDiscounts  => _offerDiscounts  ??= new GenericRepository<OfferDiscount>(_context);
    public IGenericRepository<ContactMessage>   ContactMessages => _contactMessages ??= new GenericRepository<ContactMessage>(_context);

    public IGenericRepository<Prescription>     Prescriptions     => _prescriptions     ??= new GenericRepository<Prescription>(_context);
    public IGenericRepository<PrescriptionItem> PrescriptionItems => _prescriptionItems ??= new GenericRepository<PrescriptionItem>(_context);
    public IGenericRepository<DentalChart>      DentalCharts      => _dentalCharts      ??= new GenericRepository<DentalChart>(_context);
    public IGenericRepository<ToothRecord>      ToothRecords      => _toothRecords      ??= new GenericRepository<ToothRecord>(_context);

    public IGenericRepository<ScheduleChangeRequest>           ScheduleChangeRequests          => _scheduleChangeRequests ??= new GenericRepository<ScheduleChangeRequest>(_context);
    public IGenericRepository<ServiceModificationRequest>      ServiceModificationRequests     => _serviceModRequests     ??= new GenericRepository<ServiceModificationRequest>(_context);
    public IGenericRepository<FAQModificationRequest>          FAQModificationRequests         => _faqModRequests         ??= new GenericRepository<FAQModificationRequest>(_context);
    public IGenericRepository<OfferDiscountModificationRequest> OfferDiscountModificationRequests => _offerModRequests    ??= new GenericRepository<OfferDiscountModificationRequest>(_context);
    public IGenericRepository<BranchModificationRequest>       BranchModificationRequests      => _branchModRequests      ??= new GenericRepository<BranchModificationRequest>(_context);

    public IGenericRepository<Notification> Notifications => _notifications ??= new GenericRepository<Notification>(_context);
    public IGenericRepository<SystemLog>    SystemLogs    => _systemLogs    ??= new GenericRepository<SystemLog>(_context);

    // ── Persistence ────────────────────────────────────────────────────────
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    // ── FIX-5: CancellationToken forwarded into all transaction operations ─
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is not null)
            throw new InvalidOperationException(
                "A transaction is already in progress. Nested transactions are not supported.");

        _currentTransaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No active transaction to commit.");

        try
        {
            await _context.SaveChangesAsync(ct);
            await _currentTransaction.CommitAsync(ct);
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null) return;

        try
        {
            await _currentTransaction.RollbackAsync(ct);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    // ── IDisposable ────────────────────────────────────────────────────────
    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}
