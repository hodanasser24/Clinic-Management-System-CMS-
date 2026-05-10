# DCMS — Project Context File
> **Purpose:** This file enables a new AI instance to fully resume the Dental Clinic Management System implementation. Read every section before producing any code.

---

## 1. Project Overview

**Project Name:** Dental Clinic Management System (DCMS)
**Type:** Full-stack Web API backend (no frontend in scope)
**Goal:** A multi-role clinic management platform that handles patient appointment booking, doctor scheduling, clinical reporting, prescription management, dental charting, and a governed content-modification workflow between Admins and the clinic Owner.

### Core Actors & Their Capabilities

| Role | Key Responsibilities |
|---|---|
| **Patient** | Book appointments, view reports/prescriptions, access dental chart, send contact messages |
| **Doctor** | Manage appointments, create reports, write prescriptions, update dental chart, mark urgent cases |
| **Owner** | Extends Doctor — full supervisory authority, approves all Admin modification requests, manages accounts |
| **Admin** | Confirms/rejects appointments, submits modification requests, generates reports, handles general messages |
| **Guest** | Browse only — ephemeral 30-min session, can submit General contact messages only |

### Source Documents (all have been fully read and incorporated)
- `SRS_updated_DCMS.pdf` — 58 numbered Business Rules (BR-1 to BR-58)
- `Class_Specification_DCMS.pdf` — All 25 class definitions with properties and types
- `Class_Diagram_DCMS.pdf` — Visual UML with all relationships
- `USE_CASES_DCMS.pdf` — Full use case descriptions
- `UI__Ux_initiate_design_DMCS.pdf` — UI/UX wireframes (reference only, backend focus)

---

## 2. Tech Stack & Architecture

### Technologies
```
Runtime:        .NET Core 8 / 9
Language:       C# 12
ORM:            Entity Framework Core (Code-First)
Database:       SQL Server (LocalDB for dev, SQL Server for prod)
Auth:           JWT Bearer Tokens (System.IdentityModel.Tokens.Jwt)
Password Hash:  BCrypt.Net-Next
Validation:     FluentValidation
Mapping:        AutoMapper (or Mapster — either is acceptable)
Background Jobs: Hangfire (or Quartz.NET)
API Docs:       Swashbuckle (Swagger UI with Bearer token support)
Testing:        xUnit + Moq
```

### Architectural Pattern: Clean Architecture (4 Projects)

```
DCMS.sln
├── DCMS.Domain          (Class Library)     ← innermost, zero dependencies
├── DCMS.Application     (Class Library)     ← depends on Domain only
├── DCMS.Infrastructure  (Class Library)     ← depends on Application + Domain
└── DCMS.WebAPI          (ASP.NET Core)      ← depends on Application (+ Infrastructure at DI root)
```

**Patterns in use:**
- Repository Pattern (generic + specialized)
- Unit of Work Pattern (wraps all repositories + transactions)
- Service Layer Pattern (Application services implement interfaces)
- SOLID principles throughout
- TPH (Table-Per-Hierarchy) for the User inheritance tree

---

## 3. Current Progress

### ✅ Completed: Sprint 1 — Core Identity (Slides 1.0–1.8 generated)

**All 25 Domain Entities defined** (properties, types, relationships, Fluent API rules):

#### User Hierarchy (TPH — all in `Users` table)
| # | Class | File | Notes |
|---|---|---|---|
| 1 | `User` (abstract) | `DCMS.Domain/Entities/User.cs` | Base: Id, Name, Email, PasswordHash, Phone, Role, IsFirstLogin, IsActive |
| 2 | `Patient` | `DCMS.Domain/Entities/Patient.cs` | + DateOfBirth, MedicalHistory |
| 3 | `Doctor` | `DCMS.Domain/Entities/Doctor.cs` | + Specialization, Qualification, Bio, PhotoUrl, ExperienceYears |
| 4 | `Owner` | `DCMS.Domain/Entities/Owner.cs` | Extends Doctor — no new scalar props, only additional nav properties |
| 5 | `Admin` | `DCMS.Domain/Entities/Admin.cs` | No new scalar props — role enforced via JWT claims |
| 6 | `Guest` | `DCMS.Domain/Entities/Guest.cs` | + SessionId (string, unique, MaxLen 128) |

#### Clinic Operations Entities (Sprint 2 — Slides 2.1–2.5 generated)
| # | Class | File | Key Detail |
|---|---|---|---|
| 7 | `Appointment` | `DCMS.Domain/Entities/Appointment.cs` | Self-referencing FK (PreviousAppointmentId), filtered unique index on DoctorId+Date+StartTime |
| 8 | `Schedule` | `DCMS.Domain/Entities/Schedule.cs` | Unique index: DoctorId+BranchId+DayOfWeek |
| 9 | `ScheduleChangeRequest` | `DCMS.Domain/Entities/ScheduleChangeRequest.cs` | Dual approval (Doctor + Owner), auto-expires 24h |
| 10 | `Branch` | `DCMS.Domain/Entities/Branch.cs` | Name, Location, Phone, WorkingHours |
| 11 | `Service` | `DCMS.Domain/Entities/Service.cs` | Price as decimal(10,2) |
| 12 | `OfferDiscount` | `DCMS.Domain/Entities/OfferDiscount.cs` | ServiceId nullable (SetNull on delete), IsActive Owner-only |
| 13 | `FAQ` | `DCMS.Domain/Entities/FAQ.cs` | Question + Answer — changes via FAQModificationRequest |
| 14 | `ContactMessage` | `DCMS.Domain/Entities/ContactMessage.cs` | Routed by Type: General→Admin, Complaint/Suggestion→Owner |

#### Remaining 11 Entities (defined in blueprint, slides NOT YET generated)
| # | Class | Sprint |
|---|---|---|
| 15 | `Prescription` | Sprint 3 |
| 16 | `PrescriptionItem` | Sprint 3 |
| 17 | `Report` | Sprint 3 |
| 18 | `ServiceModificationRequest` | Sprint 3 |
| 19 | `FAQModificationRequest` | Sprint 3 |
| 20 | `OfferDiscountModificationRequest` | Sprint 3 |
| 21 | `BranchModificationRequest` | Sprint 3 |
| 22 | `Notification` | Sprint 4 |
| 23 | `SystemLog` | Sprint 4 |
| 24 | `DentalChart` | Sprint 3 |
| 25 | `ToothRecord` | Sprint 3 |

### All 14 Enums Defined
```
DCMS.Domain/Enums/
├── UserRole.cs              → Patient, Doctor, Owner, Admin, Guest
├── AppointmentStatus.cs     → Pending, Confirmed, Rejected, Cancelled, Completed
├── AttendanceStatus.cs      → Attended, Absent, NotMarked
├── CaseStatus.cs            → Completed, NeedsFollowUp
├── ContactMessageType.cs    → General, Complaint, Suggestion
├── ContactMessageStatus.cs  → New, InProgress, Resolved
├── MedicationRoute.cs       → Oral, Topical, Injection, Mouthwash, Other
├── NotificationPriority.cs  → Normal, High
├── NotificationType.cs      → 27 values (all appointment/system events)
├── RequestStatus.cs         → Pending, Approved, Rejected, Expired
├── RequestType.cs           → Add, Update, Delete
├── SenderType.cs            → Patient, Guest
├── ToothStatus.cs           → Healthy, Decayed, Filled, Missing, (+ more)
└── TreatmentType.cs         → Cleaning, Filling, Extraction, (+ more)
```

---

## 4. Critical Code Snippets

### 4.1 BaseEntity (every entity inherits this)
```csharp
// DCMS.Domain/Common/BaseEntity.cs
namespace DCMS.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

### 4.2 User Hierarchy (TPH)
```csharp
// DCMS.Domain/Entities/User.cs
public abstract class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public bool IsFirstLogin { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<SystemLog> SystemLogs { get; set; } = [];
}

public class Patient : User
{
    public DateTime DateOfBirth { get; set; }
    public string? MedicalHistory { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<Report> Reports { get; set; } = [];
    public ICollection<ContactMessage> ContactMessages { get; set; } = [];
    public DentalChart? DentalChart { get; set; }
}

public class Doctor : User
{
    public string Specialization { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public int ExperienceYears { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<Schedule> Schedules { get; set; } = [];
    public ICollection<Report> Reports { get; set; } = [];
    public ICollection<ScheduleChangeRequest> ScheduleChangeRequests { get; set; } = [];
}

public class Owner : Doctor
{
    public ICollection<ScheduleChangeRequest> ApprovedScheduleChanges { get; set; } = [];
    public ICollection<ServiceModificationRequest> ApprovedServiceRequests { get; set; } = [];
    public ICollection<FAQModificationRequest> ApprovedFAQRequests { get; set; } = [];
    public ICollection<OfferDiscountModificationRequest> ApprovedOfferRequests { get; set; } = [];
    public ICollection<BranchModificationRequest> ApprovedBranchRequests { get; set; } = [];
}

public class Admin : User
{
    public ICollection<ScheduleChangeRequest> ScheduleChangeRequests { get; set; } = [];
    public ICollection<ServiceModificationRequest> ServiceModificationRequests { get; set; } = [];
    public ICollection<FAQModificationRequest> FAQModificationRequests { get; set; } = [];
    public ICollection<OfferDiscountModificationRequest> OfferDiscountModificationRequests { get; set; } = [];
    public ICollection<BranchModificationRequest> BranchModificationRequests { get; set; } = [];
}

public class Guest : User
{
    public string SessionId { get; set; } = string.Empty;
}
```

### 4.3 Appointment Entity (most complex)
```csharp
// DCMS.Domain/Entities/Appointment.cs
public class Appointment : BaseEntity
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int BranchId { get; set; }
    public int ServiceId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public AttendanceStatus AttendanceStatus { get; set; } = AttendanceStatus.NotMarked;
    public bool IsUrgent { get; set; } = false;
    public int? UrgentMarkedBy { get; set; }          // FK → Doctor
    public DateTime? UrgentMarkedDate { get; set; }
    public bool FollowUpFlag { get; set; } = false;
    public int? PreviousAppointmentId { get; set; }   // Self-referencing FK
    public string? Notes { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public int? ConfirmedBy { get; set; }             // FK → Admin

    // Navigation
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public Appointment? PreviousAppointment { get; set; }
    public Doctor? UrgentMarkedByDoctor { get; set; }
    public Report? Report { get; set; }
}
```

### 4.4 ApplicationDbContext — Full EF Core Configuration
```csharp
// DCMS.Infrastructure/Data/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<FAQ> FAQs => Set<FAQ>();
    public DbSet<OfferDiscount> OfferDiscounts => Set<OfferDiscount>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<ScheduleChangeRequest> ScheduleChangeRequests => Set<ScheduleChangeRequest>();
    public DbSet<ServiceModificationRequest> ServiceModificationRequests => Set<ServiceModificationRequest>();
    public DbSet<FAQModificationRequest> FAQModificationRequests => Set<FAQModificationRequest>();
    public DbSet<OfferDiscountModificationRequest> OfferDiscountModificationRequests => Set<OfferDiscountModificationRequest>();
    public DbSet<BranchModificationRequest> BranchModificationRequests => Set<BranchModificationRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    public DbSet<DentalChart> DentalCharts => Set<DentalChart>();
    public DbSet<ToothRecord> ToothRecords => Set<ToothRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── USER HIERARCHY (TPH) ──────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasDiscriminator<UserRole>("Role")
                  .HasValue<Patient>(UserRole.Patient)
                  .HasValue<Doctor>(UserRole.Doctor)
                  .HasValue<Owner>(UserRole.Owner)
                  .HasValue<Admin>(UserRole.Admin)
                  .HasValue<Guest>(UserRole.Guest);
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(150);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
            entity.Property(u => u.Role).HasConversion<string>().IsRequired();
            entity.Property(u => u.IsFirstLogin).HasDefaultValue(true);
            entity.Property(u => u.IsActive).HasDefaultValue(true);
        });
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.Property(d => d.Specialization).IsRequired().HasMaxLength(150);
            entity.Property(d => d.Qualification).IsRequired().HasMaxLength(300);
            entity.Property(d => d.Bio).HasMaxLength(1000);
            entity.Property(d => d.PhotoUrl).HasMaxLength(500);
        });
        modelBuilder.Entity<Guest>(entity =>
        {
            entity.Property(g => g.SessionId).IsRequired().HasMaxLength(128);
            entity.HasIndex(g => g.SessionId).IsUnique();
        });

        // ── APPOINTMENT ───────────────────────────────────────────
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(30).HasDefaultValue(AppointmentStatus.Pending);
            entity.Property(a => a.AttendanceStatus).HasConversion<string>().HasMaxLength(20).HasDefaultValue(AttendanceStatus.NotMarked);
            entity.Property(a => a.Notes).HasMaxLength(1000);
            entity.Property(a => a.IsUrgent).HasDefaultValue(false);

            entity.HasOne(a => a.Patient).WithMany(p => p.Appointments)
                  .HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Doctor).WithMany(d => d.Appointments)
                  .HasForeignKey(a => a.DoctorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Branch).WithMany(b => b.Appointments)
                  .HasForeignKey(a => a.BranchId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Service).WithMany(s => s.Appointments)
                  .HasForeignKey(a => a.ServiceId).OnDelete(DeleteBehavior.Restrict);

            // Self-referencing follow-up
            entity.HasOne(a => a.PreviousAppointment).WithMany()
                  .HasForeignKey(a => a.PreviousAppointmentId)
                  .IsRequired(false).OnDelete(DeleteBehavior.NoAction);

            // UrgentMarkedBy → Doctor (nullable)
            entity.HasOne(a => a.UrgentMarkedByDoctor).WithMany()
                  .HasForeignKey(a => a.UrgentMarkedBy)
                  .IsRequired(false).OnDelete(DeleteBehavior.NoAction);

            // Filtered unique index — prevents double-booking (BR-2)
            entity.HasIndex(a => new { a.DoctorId, a.Date, a.StartTime })
                  .HasFilter("[Status] NOT IN ('Cancelled','Rejected')");
        });

        // ── REPORT ────────────────────────────────────────────────
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Diagnosis).IsRequired().HasMaxLength(2000);
            entity.Property(r => r.Treatment).HasMaxLength(2000);
            entity.Property(r => r.InternalNotes).HasMaxLength(3000);
            entity.Property(r => r.CaseStatus).HasConversion<string>().HasMaxLength(30);

            entity.HasOne(r => r.Patient).WithMany(p => p.Reports)
                  .HasForeignKey(r => r.PatientId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Doctor).WithMany(d => d.Reports)
                  .HasForeignKey(r => r.DoctorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Appointment).WithOne(a => a.Report)
                  .HasForeignKey<Report>(r => r.AppointmentId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => r.AppointmentId).IsUnique(); // one report per appointment
        });

        // ── PRESCRIPTION ──────────────────────────────────────────
        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.GeneralInstructions).HasMaxLength(2000);
            entity.HasOne(p => p.Report).WithOne(r => r.Prescription)
                  .HasForeignKey<Prescription>(p => p.ReportId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(p => p.ReportId).IsUnique();
        });
        modelBuilder.Entity<PrescriptionItem>(entity =>
        {
            entity.HasKey(pi => pi.Id);
            entity.Property(pi => pi.MedicationName).IsRequired().HasMaxLength(200);
            entity.Property(pi => pi.Dosage).IsRequired().HasMaxLength(200);
            entity.Property(pi => pi.Route).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(pi => pi.Prescription).WithMany(p => p.Items)
                  .HasForeignKey(pi => pi.PrescriptionId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── SCHEDULE ──────────────────────────────────────────────
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.DayOfWeek).HasConversion<string>().HasMaxLength(15);
            entity.Property(s => s.SessionDurationMinutes).IsRequired();
            entity.HasOne(s => s.Doctor).WithMany(d => d.Schedules)
                  .HasForeignKey(s => s.DoctorId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Branch).WithMany(b => b.Schedules)
                  .HasForeignKey(s => s.BranchId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(s => new { s.DoctorId, s.BranchId, s.DayOfWeek }).IsUnique();
        });

        // ── SCHEDULE CHANGE REQUEST ───────────────────────────────
        modelBuilder.Entity<ScheduleChangeRequest>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(20)
                  .HasDefaultValue(RequestStatus.Pending);
            entity.HasOne(r => r.Schedule).WithMany()
                  .HasForeignKey(r => r.ScheduleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Admin).WithMany(a => a.ScheduleChangeRequests)
                  .HasForeignKey(r => r.AdminId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Doctor).WithMany(d => d.ScheduleChangeRequests)
                  .HasForeignKey(r => r.DoctorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Owner).WithMany()
                  .HasForeignKey(r => r.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── OFFER DISCOUNT ────────────────────────────────────────
        modelBuilder.Entity<OfferDiscount>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.DiscountPercentage).HasColumnType("decimal(5,2)");
            entity.HasOne(o => o.Branch).WithMany(b => b.OfferDiscounts)
                  .HasForeignKey(o => o.BranchId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(o => o.Service).WithMany(s => s.OfferDiscounts)
                  .HasForeignKey(o => o.ServiceId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        });

        // ── 4 MODIFICATION REQUEST TYPES ─────────────────────────
        modelBuilder.Entity<ServiceModificationRequest>(entity =>
        {
            entity.Property(r => r.RequestType).HasConversion<string>().HasMaxLength(20);
            entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(RequestStatus.Pending);
            entity.Property(r => r.ProposedPrice).HasColumnType("decimal(10,2)");
            entity.HasOne(r => r.Service).WithMany().HasForeignKey(r => r.ServiceId)
                  .IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<FAQModificationRequest>(entity =>
        {
            entity.Property(r => r.RequestType).HasConversion<string>().HasMaxLength(20);
            entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(RequestStatus.Pending);
            entity.HasOne(r => r.FAQ).WithMany().HasForeignKey(r => r.FAQId)
                  .IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<OfferDiscountModificationRequest>(entity =>
        {
            entity.Property(r => r.RequestType).HasConversion<string>().HasMaxLength(20);
            entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(RequestStatus.Pending);
            entity.Property(r => r.ProposedDiscountPercentage).HasColumnType("decimal(5,2)");
            entity.HasOne(r => r.OfferDiscount).WithMany().HasForeignKey(r => r.OfferId)
                  .IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<BranchModificationRequest>(entity =>
        {
            entity.Property(r => r.RequestType).HasConversion<string>().HasMaxLength(20);
            entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(RequestStatus.Pending);
            entity.HasOne(r => r.Branch).WithMany().HasForeignKey(r => r.BranchId)
                  .IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        });

        // ── NOTIFICATION ──────────────────────────────────────────
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Type).HasConversion<string>().HasMaxLength(60);
            entity.Property(n => n.Priority).HasConversion<string>().HasMaxLength(10)
                  .HasDefaultValue(NotificationPriority.Normal);
            entity.Property(n => n.IsRead).HasDefaultValue(false);
            entity.HasOne(n => n.User).WithMany(u => u.Notifications)
                  .HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── SYSTEM LOG ────────────────────────────────────────────
        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(l => l.ActionType).IsRequired().HasMaxLength(50);
            entity.Property(l => l.IPAddress).HasMaxLength(45);
            entity.Property(l => l.RetentionDate).IsRequired(); // UtcNow + 2 years (BR-6.5)
            entity.HasOne(l => l.User).WithMany(u => u.SystemLogs)
                  .HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(l => new { l.UserId, l.Date });
            entity.HasIndex(l => l.RetentionDate);
        });

        // ── DENTAL CHART & TOOTH RECORD ───────────────────────────
        modelBuilder.Entity<DentalChart>(entity =>
        {
            entity.HasKey(dc => dc.Id);
            entity.HasOne(dc => dc.Patient).WithOne(p => p.DentalChart)
                  .HasForeignKey<DentalChart>(dc => dc.PatientId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(dc => dc.PatientId).IsUnique();
        });
        modelBuilder.Entity<ToothRecord>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(t => t.TreatmentType).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(t => t.DentalChart).WithMany(dc => dc.ToothRecords)
                  .HasForeignKey(t => t.ChartId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(t => new { t.ChartId, t.ToothNumber }).IsUnique();
        });

        // ── CONTACT MESSAGE ───────────────────────────────────────
        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Type).HasConversion<string>().HasMaxLength(20);
            entity.Property(m => m.Status).HasConversion<string>().HasMaxLength(20)
                  .HasDefaultValue(ContactMessageStatus.New);
            entity.Property(m => m.SenderType).HasConversion<string>().HasMaxLength(10);
            entity.Property(m => m.Name).IsRequired().HasMaxLength(150);
            entity.Property(m => m.Email).IsRequired().HasMaxLength(255);
            entity.Property(m => m.Message).IsRequired().HasMaxLength(3000);
            entity.Property(m => m.SenderId).IsRequired(false);
        });

        // ── SERVICE & FAQ & BRANCH ────────────────────────────────
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Price).HasColumnType("decimal(10,2)");
        });
        modelBuilder.Entity<FAQ>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Question).IsRequired().HasMaxLength(500);
            entity.Property(f => f.Answer).IsRequired().HasMaxLength(2000);
        });
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Name).IsRequired().HasMaxLength(200);
            entity.Property(b => b.Phone).HasMaxLength(20);
        });
    }
}
```

### 4.5 Generic Repository Interface
```csharp
// DCMS.Domain/Interfaces/Repositories/IGenericRepository.cs
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
}
```

### 4.6 Unit of Work Interface
```csharp
// DCMS.Domain/Interfaces/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    IAppointmentRepository Appointments { get; }
    IPatientRepository Patients { get; }
    IDoctorRepository Doctors { get; }
    IScheduleRepository Schedules { get; }
    IReportRepository Reports { get; }
    IGenericRepository<Notification> Notifications { get; }
    IGenericRepository<SystemLog> SystemLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
```

### 4.7 Appointment Service Interface
```csharp
// DCMS.Application/Interfaces/IAppointmentService.cs
public interface IAppointmentService
{
    Task<AppointmentResponseDto> BookAppointmentAsync(AppointmentRequestDto request, int patientId, CancellationToken ct = default);
    Task<AppointmentResponseDto> ConfirmAppointmentAsync(int appointmentId, int adminId, CancellationToken ct = default);
    Task<AppointmentResponseDto> RejectAppointmentAsync(int appointmentId, int adminId, string reason, CancellationToken ct = default);
    Task<AppointmentResponseDto> CancelAppointmentAsync(int appointmentId, int cancelledByUserId, CancellationToken ct = default);
    Task<AppointmentResponseDto> MarkAsUrgentAsync(int appointmentId, int doctorId, CancellationToken ct = default);
    Task<AppointmentResponseDto> UnmarkUrgentAsync(int appointmentId, int doctorId, CancellationToken ct = default);
    Task MarkAttendanceAsync(int appointmentId, AttendanceStatus status, int markedByUserId, CancellationToken ct = default);
    Task<IEnumerable<string>> GetAvailableTimeSlotsAsync(int doctorId, int branchId, DateOnly date, CancellationToken ct = default);
    Task<PagedResultDto<AppointmentResponseDto>> GetAppointmentsAsync(AppointmentFilterDto filter, CancellationToken ct = default);
}
```

---

## 5. Roadmap & Next Steps

### What was produced in the previous session
1. ✅ **Technical Blueprint DOCX** — full 5-section document covering all 25 classes, ApplicationDbContext, AppointmentService, and the 4-sprint roadmap
2. ✅ **Sprint 1 Visual Slides (1.0–1.8)** — User hierarchy, all 6 identity entities, 14 enums, implementation checklist
3. ✅ **Sprint 2 Visual Slides (2.1–2.5)** — Appointment, Schedule, Branch, Service, OfferDiscount, FAQ, ContactMessage, implementation checklist

### ⏭ Immediate Next Task: Sprint 3 Visual Slides

Generate slide-ready implementation cards for **Sprint 3 — Clinical Module & Approval Workflows**, covering these 11 entities in the same visual format used for Sprints 1 and 2:

#### Sprint 3 Entities to slide-ify (in order)
| Slide | Entity | Key Detail |
|---|---|---|
| 3.1 | `Report.cs` | 1:1 with Appointment (unique index), InternalNotes visible to doctors only (BR-57) |
| 3.2 | `Prescription.cs` | 1:1 with Report (optional), cascades on Report delete |
| 3.3 | `PrescriptionItem.cs` | M:1 to Prescription, MedicationName+Dosage required (BR-40) |
| 3.4 | `DentalChart.cs` | 1:1 with Patient (unique index), cascade delete |
| 3.5 | `ToothRecord.cs` | M:1 to DentalChart, unique (ChartId + ToothNumber) |
| 3.6 | `ServiceModificationRequest.cs` | Admin submits, Owner approves, ProposedPrice decimal(10,2) |
| 3.7 | `FAQModificationRequest.cs` | RequestType: Add/Update/Delete |
| 3.8 | `OfferDiscountModificationRequest.cs` | Includes all proposed Offer fields |
| 3.9 | `BranchModificationRequest.cs` | Includes all proposed Branch fields |
| 3.10 | `ScheduleChangeRequest.cs` | BOTH Doctor AND Owner must approve (BR-6), auto-expire 24h (BR-7) |
| 3.11 | Sprint 3 File Checklist | All files with paths, layer, and type |

#### Sprint 4 Entities (after Sprint 3 is done)
| Slide | Entity | Key Detail |
|---|---|---|
| 4.1 | `Notification.cs` | UserId FK, Priority enum, IsRead, cascade delete |
| 4.2 | `SystemLog.cs` | RetentionDate = CreatedAt + 2yr, indexed, Owner sees all / Admin sees schedule only |
| 4.3 | Background Jobs | ScheduleChangeRequest auto-expiry (24h), Offer auto-deactivation (daily), Log cleanup (monthly) |
| 4.4 | Dashboard + Analytics | DashboardService, revenue/attendance/cancellation reports |
| 4.5 | Sprint 4 File Checklist | All remaining 18 files |

---

## 6. Known Issues, Design Decisions & Constraints

### Critical Design Decisions (do not change these)

| Decision | Rationale |
|---|---|
| **TPH for all User subtypes** | Avoids JOINs on every auth query; discriminator = Role enum stored as string |
| **All enums stored as `string` in DB** | `HasConversion<string>()` on every enum — human-readable schema, no magic integers |
| **`DateOnly` and `TimeOnly` for schedule/appointment** | .NET 6+ types, fully supported by EF Core 8 on SQL Server |
| **`DeleteBehavior.Restrict` on all FK relationships** | Preserve historical data — never cascade delete medical records |
| **`DeleteBehavior.NoAction` on self-ref and UrgentMarkedBy** | Prevents SQL Server circular cascade error |
| **`DeleteBehavior.SetNull` on OfferDiscount.ServiceId** | Offer persists even if the service it targets is removed |
| **`DeleteBehavior.Cascade` only on Prescription→PrescriptionItem and DentalChart→ToothRecord** | Child records are meaningless without their parent |
| **Filtered unique index on Appointment** | `HasFilter("[Status] NOT IN ('Cancelled','Rejected')")` — allows rebooking cancelled slots |
| **`InternalNotes` filtered at DTO level, NOT DB level** | Report.InternalNotes is always fetched; the Application layer omits it from PatientResponseDto and AdminResponseDto. Only DoctorResponseDto includes it (BR-57) |
| **Owner extends Doctor (not User)** | The clinic owner is always a practicing doctor — no separate Owner-only account type |
| **Guest session stored in DB** | SessionId unique index enables server-side session invalidation on login/registration |
| **RetentionDate stored on SystemLog** | `RetentionDate = DateTime.UtcNow.AddYears(2)` set at creation — background job queries this index for cleanup (BR-6.5) |
| **Slot generation is service logic** | Available time slots are computed in `ScheduleService.GetAvailableTimeSlotsAsync()` — NOT pre-stored in DB (Design Note 6) |

### Business Rules That Require Special Implementation Attention

```
BR-2  → Filtered unique index in DB (already in ApplicationDbContext)
BR-6  → ScheduleChangeRequest needs BOTH DoctorApproved + OwnerApproved bool fields
BR-7  → Hangfire recurring job every 15 min sets SCR status to Expired if pending > 24h
BR-9  → Appointment.PreviousAppointmentId self-ref, DeleteBehavior.NoAction
BR-14 → AppointmentService.MarkAsUrgentAsync checks appointment.DoctorId == requestingDoctorId
BR-23 → Daily Hangfire job: OfferDiscount.IsActive = false where EndDate < DateOnly.FromDateTime(UtcNow)
BR-37 → MarkAttendanceAsync validates: DateTime.UtcNow > appointment.Date + appointment.EndTime
BR-39 → Prescription can only be created if a Report already exists for that appointment
BR-40 → PrescriptionItem: MedicationName + Dosage are both Required — enforce in FluentValidation
BR-49 → EITHER Doctor OR Owner can reject a ScheduleChangeRequest (no consensus needed for rejection)
BR-50 → Guest.SessionId generated as Guid.NewGuid().ToString() in GuestSessionMiddleware
BR-56 → UnmarkUrgentAsync: throw ForbiddenException if appointment.UrgentMarkedBy != requestingDoctorId
BR-57 → InternalNotes hidden from Patient and Admin roles at DTO mapping level
BR-58 → OfferDiscount activate/deactivate endpoint: [Authorize(Roles = "Owner")] only
```

### Solved Gotchas (avoid repeating)
- **Multiple cascade paths**: SQL Server throws if two FK paths cascade to the same table. Fix: use `DeleteBehavior.NoAction` on the second path (UrgentMarkedBy, PreviousAppointmentId).
- **TPH discriminator**: Must call `entity.HasDiscriminator<UserRole>("Role")` on the abstract `User` entity, not on any subtype.
- **`DateOnly`/`TimeOnly` EF mapping**: No extra package needed in EF Core 8 for SQL Server — maps natively to `date` and `time` column types.
- **Self-referencing navigation**: `WithMany()` with empty arg (no parameter) is correct for the non-owning side of a self-reference.
- **Unique index on nullable FK**: `HasIndex(p => p.ReportId).IsUnique()` works correctly on nullable int — EF Core allows multiple NULLs by default in SQL Server (nulls are not considered equal for unique constraints).

---

## 7. Full File Tree (all 58 files across 4 sprints)

```
DCMS.Domain/
├── Common/BaseEntity.cs
├── Enums/ [14 files]
├── Entities/ [25 files]
└── Interfaces/
    ├── IUnitOfWork.cs
    └── Repositories/ [IGenericRepository + 5 specialized]

DCMS.Application/
├── DTOs/ [Auth, Appointments, Patients, Doctors, Reports, Common]
├── Interfaces/ [9 service interfaces]
├── Services/ [9 service implementations]
├── Validators/ [AppointmentRequestValidator + CreateReportValidator]
└── Mappings/MappingProfile.cs

DCMS.Infrastructure/
├── Data/ApplicationDbContext.cs
├── Data/Migrations/ [auto-generated]
├── Repositories/ [GenericRepository + 5 specialized]
├── UnitOfWork/UnitOfWork.cs
├── Identity/JwtTokenService.cs
├── Services/EmailNotificationService.cs
└── DependencyInjection/InfrastructureServiceRegistration.cs

DCMS.WebAPI/
├── Controllers/ [11 controllers]
├── Middleware/ [ExceptionHandling, AuditLogging, GuestSession]
├── Extensions/ServiceCollectionExtensions.cs
├── Program.cs
└── appsettings.json
```

---

*Generated from a multi-session DCMS design and implementation planning conversation. All decisions are derived from the original SRS, Class Specification, Use Cases, Class Diagram, and UI/UX PDF documents.*
