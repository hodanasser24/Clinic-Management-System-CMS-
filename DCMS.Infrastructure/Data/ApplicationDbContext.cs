using DCMS.Domain.Common;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DCMS.Infrastructure.Data;

/// <summary>
/// EF Core DbContext for DCMS.
/// FIX-1  — SaveChangesAsync sets CreatedAt/UpdatedAt via ChangeTracker.
/// FIX-2  — Appointment.ConfirmedBy FK fully configured (DeleteBehavior.Restrict).
/// FIX-3  — AdminId + OwnerId FKs explicitly configured on all 4 modification request types.
/// IMPROVE-3 — ScheduleChangeRequest.DoctorId renamed to RequestingDoctorId.
/// BR-6   — DoctorApproved/OwnerApproved HasDefaultValue(false).
/// Phase 4 indexes: Notification(UserId,IsRead), Appointment(PatientId,Date),
///                  ContactMessage(Type), ContactMessage(Status).
/// </summary>
using DCMS.Application.Interfaces;
public class ApplicationDbContext : DbContext, IAppDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // ── DbSets ─────────────────────────────────────────────────────────────
    public DbSet<User>                              Users                               => Set<User>();
    public DbSet<Patient>                           Patients                            => Set<Patient>();
    public DbSet<Doctor>                            Doctors                             => Set<Doctor>();
    public DbSet<Owner>                             Owners                              => Set<Owner>();
    public DbSet<Admin>                             Admins                              => Set<Admin>();
    public DbSet<Guest>                             Guests                              => Set<Guest>();
    public DbSet<Appointment>                       Appointments                        => Set<Appointment>();
    public DbSet<Service>                           Services                            => Set<Service>();
    public DbSet<FAQ>                               FAQs                                => Set<FAQ>();
    public DbSet<OfferDiscount>                     OfferDiscounts                      => Set<OfferDiscount>();
    public DbSet<Branch>                            Branches                            => Set<Branch>();
    public DbSet<Prescription>                      Prescriptions                       => Set<Prescription>();
    public DbSet<PrescriptionItem>                  PrescriptionItems                   => Set<PrescriptionItem>();
    public DbSet<Report>                            Reports                             => Set<Report>();
    public DbSet<ContactMessage>                    ContactMessages                     => Set<ContactMessage>();
    public DbSet<Schedule>                          Schedules                           => Set<Schedule>();
    public DbSet<ScheduleChangeRequest>             ScheduleChangeRequests              => Set<ScheduleChangeRequest>();
    public DbSet<ServiceModificationRequest>        ServiceModificationRequests         => Set<ServiceModificationRequest>();
    public DbSet<FAQModificationRequest>            FAQModificationRequests             => Set<FAQModificationRequest>();
    public DbSet<OfferDiscountModificationRequest>  OfferDiscountModificationRequests   => Set<OfferDiscountModificationRequest>();
    public DbSet<BranchModificationRequest>         BranchModificationRequests          => Set<BranchModificationRequest>();
    public DbSet<Notification>                      Notifications                       => Set<Notification>();
    public DbSet<SystemLog>                         SystemLogs                          => Set<SystemLog>();
    public DbSet<DentalChart>                       DentalCharts                        => Set<DentalChart>();
    public DbSet<ToothRecord>                       ToothRecords                        => Set<ToothRecord>();
    public DbSet<Revenue>                           Revenues                            => Set<Revenue>();

    // ── FIX-1: Timestamp management ───────────────────────────────────────
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        StampTimestamps();
        return base.SaveChanges();
    }

    private void StampTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (EntityEntry<BaseEntity> entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }
    }

    // ── Fluent API ─────────────────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUserHierarchy(modelBuilder);
        ConfigureAppointment(modelBuilder);
        ConfigureReport(modelBuilder);
        ConfigurePrescription(modelBuilder);
        ConfigureSchedule(modelBuilder);
        ConfigureScheduleChangeRequest(modelBuilder);
        ConfigureOfferDiscount(modelBuilder);
        ConfigureModificationRequests(modelBuilder);
        ConfigureNotification(modelBuilder);
        ConfigureSystemLog(modelBuilder);
        ConfigureDentalChart(modelBuilder);
        ConfigureContactMessage(modelBuilder);
        ConfigureServiceFaqBranch(modelBuilder);
        ConfigureRevenue(modelBuilder);
    }

    // ── USER HIERARCHY (TPH) ──────────────────────────────────────────────
    private static void ConfigureUserHierarchy(ModelBuilder mb)
    {
        mb.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasDiscriminator<UserRole>("Role")
             .HasValue<Patient>(UserRole.Patient)
             .HasValue<Doctor>(UserRole.Doctor)
             .HasValue<Owner>(UserRole.Owner)
             .HasValue<Admin>(UserRole.Admin)
             .HasValue<Guest>(UserRole.Guest);
            e.HasKey(u => u.Id);
            e.Property(u => u.FullName).IsRequired().HasMaxLength(150);
            e.Property(u => u.Email).IsRequired().HasMaxLength(255);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
            e.Property(u => u.Role).HasConversion<string>().IsRequired();
            e.Property(u => u.IsFirstLogin).HasDefaultValue(true);
            e.Property(u => u.IsActive).HasDefaultValue(true);
        });

        mb.Entity<Doctor>(e =>
        {
            e.Property(d => d.Specialization).IsRequired().HasMaxLength(150);
            e.Property(d => d.Qualification).IsRequired().HasMaxLength(300);
            e.Property(d => d.Bio).HasMaxLength(1000);
            e.Property(d => d.PhotoUrl).HasMaxLength(500);
        });

        mb.Entity<Guest>(e =>
        {
            e.Property(g => g.SessionId).IsRequired().HasMaxLength(128);
            e.HasIndex(g => g.SessionId).IsUnique();
        });
    }

    // ── APPOINTMENT ───────────────────────────────────────────────────────
    private static void ConfigureAppointment(ModelBuilder mb)
    {
        mb.Entity<Appointment>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Status)
             .HasConversion<string>().HasMaxLength(30)
             .HasDefaultValue(AppointmentStatus.Pending);
            e.Property(a => a.AttendanceStatus)
             .HasConversion<string>().HasMaxLength(20)
             .HasDefaultValue(AttendanceStatus.NotMarked);
            e.Property(a => a.Notes).HasMaxLength(1000);
            e.Property(a => a.IsUrgent).HasDefaultValue(false);
            e.Property(a => a.FollowUpFlag).HasDefaultValue(false);

            e.HasOne(a => a.Patient).WithMany(p => p.Appointments)
             .HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Doctor).WithMany(d => d.Appointments)
             .HasForeignKey(a => a.DoctorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Branch).WithMany(b => b.Appointments)
             .HasForeignKey(a => a.BranchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Service).WithMany(s => s.Appointments)
             .HasForeignKey(a => a.ServiceId).OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.PreviousAppointment).WithMany()
             .HasForeignKey(a => a.PreviousAppointmentId)
             .IsRequired(false).OnDelete(DeleteBehavior.NoAction);

            e.HasOne(a => a.UrgentMarkedByDoctor).WithMany()
             .HasForeignKey(a => a.UrgentMarkedBy)
             .IsRequired(false).OnDelete(DeleteBehavior.NoAction);

            // FIX-2: ConfirmedBy → Admin FK with referential integrity
            e.HasOne(a => a.ConfirmedByAdmin).WithMany()
             .HasForeignKey(a => a.ConfirmedBy)
             .IsRequired(false).OnDelete(DeleteBehavior.Restrict);

            // BR-2: Filtered unique index — prevent double-booking
            e.HasIndex(a => new { a.DoctorId, a.Date, a.StartTime })
             .HasFilter("[Status] NOT IN ('Cancelled','Rejected')")
             .HasDatabaseName("UX_Appointment_Doctor_Date_StartTime_Active");

            // Phase 4 performance index
            e.HasIndex(a => new { a.PatientId, a.Date })
             .HasDatabaseName("IX_Appointment_PatientId_Date");
        });
    }

    // ── REPORT ────────────────────────────────────────────────────────────
    private static void ConfigureReport(ModelBuilder mb)
    {
        mb.Entity<Report>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Diagnosis).IsRequired().HasMaxLength(2000);
            e.Property(r => r.Treatment).HasMaxLength(2000);
            e.Property(r => r.InternalNotes).HasMaxLength(3000);
            e.Property(r => r.CaseStatus).HasConversion<string>().HasMaxLength(30);

            e.HasOne(r => r.Patient).WithMany(p => p.Reports)
             .HasForeignKey(r => r.PatientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Doctor).WithMany(d => d.Reports)
             .HasForeignKey(r => r.DoctorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Appointment).WithOne(a => a.Report)
             .HasForeignKey<Report>(r => r.AppointmentId).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(r => r.AppointmentId)
             .IsUnique()
             .HasDatabaseName("UX_Report_AppointmentId");
        });
    }

    // ── PRESCRIPTION ──────────────────────────────────────────────────────
    private static void ConfigurePrescription(ModelBuilder mb)
    {
        mb.Entity<Prescription>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.GeneralInstructions).HasMaxLength(2000);
            e.HasOne(p => p.Report).WithOne(r => r.Prescription)
             .HasForeignKey<Prescription>(p => p.ReportId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(p => p.ReportId)
             .IsUnique()
             .HasDatabaseName("UX_Prescription_ReportId");
        });

        mb.Entity<PrescriptionItem>(e =>
        {
            e.HasKey(pi => pi.Id);
            e.Property(pi => pi.MedicationName).IsRequired().HasMaxLength(200);
            e.Property(pi => pi.Dosage).IsRequired().HasMaxLength(200);
            e.Property(pi => pi.Frequency).HasMaxLength(200);
            e.Property(pi => pi.Duration).HasMaxLength(100);
            e.Property(pi => pi.Route).HasConversion<string>().HasMaxLength(30);
            e.Property(pi => pi.Notes).HasMaxLength(500);
            e.HasOne(pi => pi.Prescription).WithMany(p => p.Items)
             .HasForeignKey(pi => pi.PrescriptionId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    // ── SCHEDULE ──────────────────────────────────────────────────────────
    private static void ConfigureSchedule(ModelBuilder mb)
    {
        mb.Entity<Schedule>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.DayOfWeek).HasConversion<string>().HasMaxLength(15);
            e.Property(s => s.SessionDurationMinutes).IsRequired();
            e.HasOne(s => s.Doctor).WithMany(d => d.Schedules)
             .HasForeignKey(s => s.DoctorId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Branch).WithMany(b => b.Schedules)
             .HasForeignKey(s => s.BranchId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(s => new { s.DoctorId, s.BranchId, s.DayOfWeek })
             .IsUnique()
             .HasDatabaseName("UX_Schedule_Doctor_Branch_DayOfWeek");
        });
    }

    // ── SCHEDULE CHANGE REQUEST ───────────────────────────────────────────
    private static void ConfigureScheduleChangeRequest(ModelBuilder mb)
    {
        mb.Entity<ScheduleChangeRequest>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Status)
             .HasConversion<string>().HasMaxLength(20)
             .HasDefaultValue(RequestStatus.Pending);
            e.Property(r => r.Reason).HasMaxLength(1000);
            e.Property(r => r.RejectionReason).HasMaxLength(1000);
            e.Property(r => r.ProposedDayOfWeek).HasConversion<string>().HasMaxLength(15);

            // BR-6: explicit DB defaults
            e.Property(r => r.DoctorApproved).HasDefaultValue(false);
            e.Property(r => r.OwnerApproved).HasDefaultValue(false);

            e.HasOne(r => r.Schedule).WithMany()
             .HasForeignKey(r => r.ScheduleId).OnDelete(DeleteBehavior.Restrict);

            // IMPROVE-3: was DoctorId — now RequestingDoctorId / RequestingDoctor nav
            e.HasOne(r => r.RequestingDoctor).WithMany(d => d.ScheduleChangeRequests)
             .HasForeignKey(r => r.RequestingDoctorId).OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Admin).WithMany(a => a.ScheduleChangeRequests)
             .HasForeignKey(r => r.AdminId).OnDelete(DeleteBehavior.Restrict);

            // OwnerId is now required (non-nullable) per P0
            e.HasOne(r => r.Owner).WithMany(o => o.ApprovedScheduleChanges)
             .HasForeignKey(r => r.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ── OFFER DISCOUNT ────────────────────────────────────────────────────
    private static void ConfigureOfferDiscount(ModelBuilder mb)
    {
        mb.Entity<OfferDiscount>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Title).IsRequired().HasMaxLength(200);
            e.Property(o => o.Description).HasMaxLength(1000);
            e.Property(o => o.DiscountPercentage).HasColumnType("decimal(5,2)");
            e.Property(o => o.IsActive).HasDefaultValue(true);
            e.HasOne(o => o.Branch).WithMany(b => b.OfferDiscounts)
             .HasForeignKey(o => o.BranchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.Service).WithMany(s => s.OfferDiscounts)
             .HasForeignKey(o => o.ServiceId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        });
    }

    // ── 4 MODIFICATION REQUEST TYPES ─────────────────────────────────────
    /// <summary>
    /// FIX-3: All four modification request types had AdminId and OwnerId
    /// scalar FKs with no Fluent API config. EF Core would have defaulted to
    /// Cascade, risking SQL Server multiple-cascade-path errors.
    /// All eight FK relationships are now explicitly Restrict.
    /// </summary>
    private static void ConfigureModificationRequests(ModelBuilder mb)
    {
        mb.Entity<ServiceModificationRequest>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.RequestType).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.Status)
             .HasConversion<string>().HasMaxLength(20)
             .HasDefaultValue(RequestStatus.Pending);
            e.Property(r => r.ProposedName).HasMaxLength(200);
            e.Property(r => r.ProposedDescription).HasMaxLength(1000);
            e.Property(r => r.ProposedPrice).HasColumnType("decimal(10,2)");
            e.Property(r => r.Reason).HasMaxLength(1000);
            e.Property(r => r.RejectionReason).HasMaxLength(1000);
            e.HasOne(r => r.Service).WithMany()
             .HasForeignKey(r => r.ServiceId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            // FIX-3
            e.HasOne(r => r.Admin).WithMany(a => a.ServiceModificationRequests)
             .HasForeignKey(r => r.AdminId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Owner).WithMany(o => o.ApprovedServiceRequests)
             .HasForeignKey(r => r.OwnerId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<FAQModificationRequest>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.RequestType).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.Status)
             .HasConversion<string>().HasMaxLength(20)
             .HasDefaultValue(RequestStatus.Pending);
            e.Property(r => r.ProposedQuestion).HasMaxLength(500);
            e.Property(r => r.ProposedAnswer).HasMaxLength(2000);
            e.Property(r => r.Reason).HasMaxLength(1000);
            e.Property(r => r.RejectionReason).HasMaxLength(1000);
            e.HasOne(r => r.FAQ).WithMany()
             .HasForeignKey(r => r.FAQId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            // FIX-3
            e.HasOne(r => r.Admin).WithMany(a => a.FAQModificationRequests)
             .HasForeignKey(r => r.AdminId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Owner).WithMany(o => o.ApprovedFAQRequests)
             .HasForeignKey(r => r.OwnerId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<OfferDiscountModificationRequest>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.RequestType).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.Status)
             .HasConversion<string>().HasMaxLength(20)
             .HasDefaultValue(RequestStatus.Pending);
            e.Property(r => r.ProposedTitle).HasMaxLength(200);
            e.Property(r => r.ProposedDescription).HasMaxLength(1000);
            e.Property(r => r.ProposedDiscountPercentage).HasColumnType("decimal(5,2)");
            e.Property(r => r.Reason).HasMaxLength(1000);
            e.Property(r => r.RejectionReason).HasMaxLength(1000);
            e.HasOne(r => r.OfferDiscount).WithMany()
             .HasForeignKey(r => r.OfferId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            // FIX-3
            e.HasOne(r => r.Admin).WithMany(a => a.OfferDiscountModificationRequests)
             .HasForeignKey(r => r.AdminId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Owner).WithMany(o => o.ApprovedOfferRequests)
             .HasForeignKey(r => r.OwnerId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<BranchModificationRequest>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.RequestType).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.Status)
             .HasConversion<string>().HasMaxLength(20)
             .HasDefaultValue(RequestStatus.Pending);
            e.Property(r => r.ProposedName).HasMaxLength(200);
            e.Property(r => r.ProposedLocation).HasMaxLength(500);
            e.Property(r => r.ProposedPhone).HasMaxLength(20);
            e.Property(r => r.ProposedWorkingHours).HasMaxLength(500);
            e.Property(r => r.Reason).HasMaxLength(1000);
            e.Property(r => r.RejectionReason).HasMaxLength(1000);
            e.HasOne(r => r.Branch).WithMany()
             .HasForeignKey(r => r.BranchId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            // FIX-3
            e.HasOne(r => r.Admin).WithMany(a => a.BranchModificationRequests)
             .HasForeignKey(r => r.AdminId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Owner).WithMany(o => o.ApprovedBranchRequests)
             .HasForeignKey(r => r.OwnerId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ── NOTIFICATION ──────────────────────────────────────────────────────
    private static void ConfigureNotification(ModelBuilder mb)
    {
        mb.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Type).HasConversion<string>().HasMaxLength(60);
            e.Property(n => n.Priority)
             .HasConversion<string>().HasMaxLength(10)
             .HasDefaultValue(NotificationPriority.Normal);
            e.Property(n => n.Title).IsRequired().HasMaxLength(200);
            e.Property(n => n.Message).IsRequired().HasMaxLength(1000);
            e.Property(n => n.IsRead).HasDefaultValue(false);
            e.HasOne(n => n.User).WithMany(u => u.Notifications)
             .HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
            // Phase 4 index
            e.HasIndex(n => new { n.UserId, n.IsRead })
             .HasDatabaseName("IX_Notification_UserId_IsRead");
        });
    }

    // ── SYSTEM LOG ────────────────────────────────────────────────────────
    private static void ConfigureSystemLog(ModelBuilder mb)
    {
        mb.Entity<SystemLog>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.EntityType).IsRequired().HasMaxLength(100);
            e.Property(l => l.ActionType).IsRequired().HasMaxLength(200);
            e.Property(l => l.UserRole).HasMaxLength(50);
            e.Property(l => l.IPAddress).HasMaxLength(45);
            e.Property(l => l.OldValues).HasColumnType("nvarchar(max)");
            e.Property(l => l.NewValues).HasColumnType("nvarchar(max)");
            e.Property(l => l.RetentionDate).IsRequired();
            e.HasOne(l => l.User).WithMany(u => u.SystemLogs)
             .HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(l => new { l.UserId, l.Date })
             .HasDatabaseName("IX_SystemLog_UserId_Date");
            e.HasIndex(l => l.RetentionDate)
             .HasDatabaseName("IX_SystemLog_RetentionDate");
        });
    }

    // ── DENTAL CHART & TOOTH RECORD ───────────────────────────────────────
    private static void ConfigureDentalChart(ModelBuilder mb)
    {
        mb.Entity<DentalChart>(e =>
        {
            e.HasKey(dc => dc.Id);
            e.Property(dc => dc.Notes).HasMaxLength(2000);
            e.HasOne(dc => dc.Patient).WithOne(p => p.DentalChart)
             .HasForeignKey<DentalChart>(dc => dc.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(dc => dc.PatientId)
             .IsUnique()
             .HasDatabaseName("UX_DentalChart_PatientId");
        });

        mb.Entity<ToothRecord>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.ToothStatus).HasConversion<string>().HasMaxLength(30);
            e.Property(t => t.TreatmentType).HasConversion<string>().HasMaxLength(30);
            e.Property(t => t.Notes).HasMaxLength(1000);
            e.HasOne(t => t.DentalChart).WithMany(dc => dc.ToothRecords)
             .HasForeignKey(t => t.ChartId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(t => new { t.ChartId, t.ToothNumber })
             .IsUnique()
             .HasDatabaseName("UX_ToothRecord_Chart_ToothNumber");
        });
    }

    // ── CONTACT MESSAGE ───────────────────────────────────────────────────
    private static void ConfigureContactMessage(ModelBuilder mb)
    {
        mb.Entity<ContactMessage>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasIndex(m => m.Type).HasDatabaseName("IX_ContactMessage_Type");
            e.HasIndex(m => m.Status).HasDatabaseName("IX_ContactMessage_Status");
            e.HasIndex(m => m.CreatedAt).HasDatabaseName("IX_ContactMessage_CreatedAt");

            e.Property(m => m.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(m => m.Status)
             .HasConversion<string>().HasMaxLength(20)
             .HasDefaultValue(ContactMessageStatus.New);

            e.Property(m => m.SenderName).IsRequired().HasMaxLength(100);
            e.Property(m => m.SenderEmail).IsRequired().HasMaxLength(150);
            e.Property(m => m.Subject).IsRequired().HasMaxLength(200);
            e.Property(m => m.Body).IsRequired().HasMaxLength(4000);
            e.Property(m => m.ReplyBody).HasMaxLength(4000);

            e.HasOne(m => m.RepliedByUser)
             .WithMany()
             .HasForeignKey(m => m.RepliedByUserId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);

            e.HasQueryFilter(m => !m.IsArchived);
        });
    }

    // ── SERVICE, FAQ, BRANCH ──────────────────────────────────────────────
    private static void ConfigureServiceFaqBranch(ModelBuilder mb)
    {
        mb.Entity<Service>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).IsRequired().HasMaxLength(200);
            e.Property(s => s.Price).HasColumnType("decimal(10,2)");
        });

        mb.Entity<FAQ>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Question).IsRequired().HasMaxLength(500);
            e.Property(f => f.Answer).IsRequired().HasMaxLength(2000);
        });

        mb.Entity<Branch>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Name).IsRequired().HasMaxLength(200);
            e.Property(b => b.Phone).HasMaxLength(20);
            e.Property(b => b.WorkingHours).HasMaxLength(500);
        });
    }

    // ── REVENUE ───────────────────────────────────────────────────────────
    private static void ConfigureRevenue(ModelBuilder mb)
    {
        mb.Entity<Revenue>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Amount).HasColumnType("decimal(18,2)");
            e.Property(r => r.Date).IsRequired();

            e.HasOne(r => r.Appointment).WithMany()
             .HasForeignKey(r => r.AppointmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Branch).WithMany()
             .HasForeignKey(r => r.BranchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Service).WithMany()
             .HasForeignKey(r => r.ServiceId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Patient).WithMany()
             .HasForeignKey(r => r.PatientId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
