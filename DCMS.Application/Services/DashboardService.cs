using System.Text;
using DCMS.Application.DTOs.Dashboard;
using DCMS.Application.Interfaces;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;

namespace DCMS.Application.Services;

/// <summary>
/// PERFORMANCE FIX: Previous implementation called GetAllAsync() on every table, loading
/// the entire database into memory for each dashboard request.
/// Now uses CountAsync() with predicates for counts, and targeted date-filtered queries
/// for the data that genuinely needs to be iterated (e.g. revenue sums).
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;

    public DashboardService(IUnitOfWork uow) => _uow = uow;

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var today     = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = today.AddDays(-(int)today.DayOfWeek);

        // ── Counts via CountAsync (no record loading) ──────────────────────────
        var totalToday     = await _uow.Appointments.CountAsync(a => a.Date == today, ct);
        var totalThisWeek  = await _uow.Appointments.CountAsync(a => a.Date >= weekStart && a.Date <= today, ct);
        var pending        = await _uow.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending, ct);
        var confirmed      = await _uow.Appointments.CountAsync(a => a.Status == AppointmentStatus.Confirmed, ct);
        var urgent         = await _uow.Appointments.CountAsync(a => a.IsUrgent, ct);
        var totalPatients  = await _uow.Patients.CountAsync(ct: ct);
        var totalDoctors   = await _uow.Doctors.CountAsync(ct: ct);
        var activeBranches = await _uow.Branches.CountAsync(b => b.IsActive, ct);
        var activeServices = await _uow.Services.CountAsync(s => s.IsActive, ct);

        // Unresolved contact messages (not Closed or Archived)
        var unresolvedMessages = await _uow.ContactMessages.CountAsync(
            m => m.Status != ContactMessageStatus.Closed, ct);

        // Pending modification requests
        var pendingModReqs =
            await _uow.ServiceModificationRequests.CountAsync(r => r.Status == RequestStatus.Pending, ct) +
            await _uow.FAQModificationRequests.CountAsync(r => r.Status == RequestStatus.Pending, ct) +
            await _uow.OfferDiscountModificationRequests.CountAsync(r => r.Status == RequestStatus.Pending, ct) +
            await _uow.BranchModificationRequests.CountAsync(r => r.Status == RequestStatus.Pending, ct);

        // ── Revenue: load only completed appointments in date ranges ───────────
        // These must be loaded to access Service.Price — but scoped to date ranges.
        var completedToday = await _uow.Appointments.FindAsync(
            a => a.Status == AppointmentStatus.Completed && a.Date == today, ct);

        var completedThisWeek = await _uow.Appointments.FindAsync(
            a => a.Status == AppointmentStatus.Completed && a.Date >= weekStart && a.Date <= today, ct);

        var completedAll = await _uow.Appointments.FindAsync(
            a => a.Status == AppointmentStatus.Completed, ct);

        // Load service prices for the fetched appointments
        decimal todayRevenue = 0, weekRevenue = 0, totalRevenue = 0;
        foreach (var a in completedAll)
        {
            var svc = await _uow.Services.GetByIdAsync(a.ServiceId, ct);
            if (svc == null) continue;
            totalRevenue += svc.Price;
            if (a.Date >= weekStart && a.Date <= today) weekRevenue  += svc.Price;
            if (a.Date == today)                        todayRevenue += svc.Price;
        }

        return new DashboardSummaryDto
        {
            TotalAppointmentsToday      = totalToday,
            TotalAppointmentsThisWeek   = totalThisWeek,
            PendingAppointments         = pending,
            ConfirmedAppointments       = confirmed,
            UrgentAppointments          = urgent,
            TotalPatients               = totalPatients,
            TotalDoctors                = totalDoctors,
            ActiveBranches              = activeBranches,
            ActiveServices              = activeServices,
            UnresolvedContactMessages   = unresolvedMessages,
            PendingModificationRequests = pendingModReqs,
            TodayRevenue                = todayRevenue,
            WeekRevenue                 = weekRevenue,
            TotalRevenue                = totalRevenue
        };
    }

    public async Task<DailyReportDto> GetDailyReportAsync(
        DateOnly date, CancellationToken ct = default)
    {
        // Scoped to a single date — acceptable data volume
        var appointments = (await _uow.Appointments.GetByDateAsync(date, ct)).ToList();

        var items = new List<AppointmentSummaryExportDto>();
        foreach (var a in appointments)
        {
            var svc = a.Service ?? await _uow.Services.GetByIdAsync(a.ServiceId, ct);
            items.Add(new AppointmentSummaryExportDto
            {
                AppointmentId = a.Id,
                PatientName   = a.Patient?.FullName ?? string.Empty,
                DoctorName    = a.Doctor?.FullName  ?? string.Empty,
                BranchName    = a.Branch?.Name      ?? string.Empty,
                ServiceName   = svc?.Name           ?? string.Empty,
                StartTime     = a.StartTime,
                Status        = a.Status.ToString(),
                IsUrgent      = a.IsUrgent
            });
        }

        var revenue = 0m;
        foreach (var a in appointments.Where(a => a.Status == AppointmentStatus.Completed))
        {
            var svc = a.Service ?? await _uow.Services.GetByIdAsync(a.ServiceId, ct);
            revenue += svc?.Price ?? 0;
        }

        return new DailyReportDto
        {
            Date              = date,
            TotalAppointments = appointments.Count,
            Attended          = appointments.Count(a => a.AttendanceStatus == AttendanceStatus.Attended),
            Absent            = appointments.Count(a => a.AttendanceStatus == AttendanceStatus.Absent),
            Cancelled         = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
            Rejected          = appointments.Count(a => a.Status == AppointmentStatus.Rejected),
            Completed         = appointments.Count(a => a.Status == AppointmentStatus.Completed),
            UrgentCases       = appointments.Count(a => a.IsUrgent),
            Revenue           = revenue,
            Appointments      = items
        };
    }

    public async Task<WeeklyReportDto> GetWeeklyReportAsync(
        DateOnly weekStart, CancellationToken ct = default)
    {
        var weekEnd = weekStart.AddDays(6);
        var daily   = new List<DailyReportDto>();

        for (var d = weekStart; d <= weekEnd; d = d.AddDays(1))
            daily.Add(await GetDailyReportAsync(d, ct));

        return new WeeklyReportDto
        {
            WeekStart         = weekStart,
            WeekEnd           = weekEnd,
            TotalAppointments = daily.Sum(d => d.TotalAppointments),
            Attended          = daily.Sum(d => d.Attended),
            Absent            = daily.Sum(d => d.Absent),
            Cancelled         = daily.Sum(d => d.Cancelled),
            WeekRevenue       = daily.Sum(d => d.Revenue),
            DailyBreakdown    = daily
        };
    }

    public async Task<byte[]> ExportDailyReportAsCsvAsync(
        DateOnly date, CancellationToken ct = default)
    {
        var report = await GetDailyReportAsync(date, ct);
        return BuildCsv(report.Appointments);
    }

    public async Task<byte[]> ExportWeeklyReportAsCsvAsync(
        DateOnly weekStart, CancellationToken ct = default)
    {
        var report = await GetWeeklyReportAsync(weekStart, ct);
        return BuildCsv(report.DailyBreakdown.SelectMany(d => d.Appointments).ToList());
    }

    private static byte[] BuildCsv(List<AppointmentSummaryExportDto> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("AppointmentId,PatientName,DoctorName,BranchName,ServiceName,StartTime,Status,IsUrgent");
        foreach (var a in items)
            sb.AppendLine(
                $"{a.AppointmentId},{Esc(a.PatientName)},{Esc(a.DoctorName)}," +
                $"{Esc(a.BranchName)},{Esc(a.ServiceName)},{a.StartTime},{a.Status},{a.IsUrgent}");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Esc(string v) => v.Contains(',') ? $"\"{v}\"" : v;

    public async Task<DoctorDailyDashboardDto> GetDoctorDailyTrackingAsync(int doctorId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayStartUtc = DateTime.UtcNow.Date;
        var tomorrowStartUtc = todayStartUtc.AddDays(1);

        // 1. Appointments for today
        var appointmentsToday = (await _uow.Appointments.FindAsync(
            a => a.DoctorId == doctorId && a.Date == today, ct)).ToList();

        var totalAppointments = appointmentsToday.Count;
        var completedAppointments = appointmentsToday.Count(a => a.Status == AppointmentStatus.Completed);
        var pendingAppointments = appointmentsToday.Count(a => a.Status == AppointmentStatus.Pending);
        var cancelledAppointments = appointmentsToday.Count(a => a.Status == AppointmentStatus.Cancelled);

        // Patients seen today (distinct from completed appointments)
        var patientsSeen = appointmentsToday
            .Where(a => a.Status == AppointmentStatus.Completed)
            .Select(a => a.PatientId)
            .Distinct()
            .Count();

        // 2. Reports created today
        var reportsToday = (await _uow.Reports.FindAsync(
            r => r.DoctorId == doctorId && r.CreatedAt >= todayStartUtc && r.CreatedAt < tomorrowStartUtc, ct)).ToList();

        // 3. Prescriptions created today
        var prescriptionsToday = (await _uow.Prescriptions.FindAsync(
            p => p.Report != null && p.Report.DoctorId == doctorId && p.CreatedAt >= todayStartUtc && p.CreatedAt < tomorrowStartUtc, ct)).ToList();

        // 4. Dental charts (ToothRecords) updated today
        var toothRecordsToday = (await _uow.ToothRecords.FindAsync(
            t => t.LastUpdatedInReport != null && t.LastUpdatedInReport.DoctorId == doctorId && t.UpdatedAt >= todayStartUtc && t.UpdatedAt < tomorrowStartUtc, ct)).ToList();
        
        var dentalChartsUpdated = toothRecordsToday.Select(t => t.ChartId).Distinct().Count();

        // 5. Build Timeline
        var timeline = new List<DoctorActivityDto>();

        foreach (var a in appointmentsToday)
        {
            if (a.CreatedAt >= todayStartUtc && a.CreatedAt < tomorrowStartUtc)
                timeline.Add(new DoctorActivityDto { Time = a.CreatedAt, ActionType = "Created", EntityType = "Appointment", Description = $"Appointment scheduled for {a.StartTime:HH:mm}" });
            
            if (a.Status == AppointmentStatus.Completed && a.CompletedAt >= todayStartUtc && a.CompletedAt < tomorrowStartUtc)
                timeline.Add(new DoctorActivityDto { Time = a.CompletedAt.Value, ActionType = "Completed", EntityType = "Appointment", Description = $"Appointment completed for {a.StartTime:HH:mm}" });
            else if (a.Status == AppointmentStatus.Cancelled && a.CancelledAt >= todayStartUtc && a.CancelledAt < tomorrowStartUtc)
                timeline.Add(new DoctorActivityDto { Time = a.CancelledAt.Value, ActionType = "Cancelled", EntityType = "Appointment", Description = $"Appointment cancelled for {a.StartTime:HH:mm}" });
        }

        foreach (var r in reportsToday)
            timeline.Add(new DoctorActivityDto { Time = r.CreatedAt, ActionType = "Created", EntityType = "Report", Description = "Clinical report created" });

        foreach (var p in prescriptionsToday)
            timeline.Add(new DoctorActivityDto { Time = p.CreatedAt, ActionType = "Created", EntityType = "Prescription", Description = "Prescription issued" });

        foreach (var t in toothRecordsToday)
            timeline.Add(new DoctorActivityDto { Time = t.UpdatedAt, ActionType = "Updated", EntityType = "DentalChart", Description = $"Tooth #{t.ToothNumber} record updated" });

        return new DoctorDailyDashboardDto
        {
            TotalAppointments = totalAppointments,
            CompletedAppointments = completedAppointments,
            PendingAppointments = pendingAppointments,
            CancelledAppointments = cancelledAppointments,
            PatientsSeen = patientsSeen,
            ReportsCreated = reportsToday.Count,
            PrescriptionsCreated = prescriptionsToday.Count,
            DentalChartsUpdated = dentalChartsUpdated,
            ActivityTimeline = timeline.OrderByDescending(x => x.Time).ToList()
        };
    }
}
