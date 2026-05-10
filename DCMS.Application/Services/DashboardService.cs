using System.Text;
using DCMS.Application.DTOs.Dashboard;
using DCMS.Application.Interfaces;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;

namespace DCMS.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;

    public DashboardService(IUnitOfWork uow) => _uow = uow;

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = today.AddDays(-(int)today.DayOfWeek);

        var allAppointments = (await _uow.Appointments.GetAllAsync(ct)).ToList();
        var allPatients = await _uow.Patients.GetAllAsync(ct);
        var allDoctors = await _uow.Doctors.GetAllAsync(ct);
        var allBranches = await _uow.Branches.GetAllAsync(ct);
        var allServices = await _uow.Services.GetAllAsync(ct);
        var allMessages = await _uow.ContactMessages.GetAllAsync(ct);
        var allServiceReqs = await _uow.ServiceModificationRequests.GetAllAsync(ct);
        var allFaqReqs = await _uow.FAQModificationRequests.GetAllAsync(ct);
        var allOfferReqs = await _uow.OfferDiscountModificationRequests.GetAllAsync(ct);
        var allBranchReqs = await _uow.BranchModificationRequests.GetAllAsync(ct);

        // Revenue from completed appointments
        var completedAppts = allAppointments
            .Where(a => a.Status == AppointmentStatus.Completed && a.Service != null)
            .ToList();

        return new DashboardSummaryDto
        {
            TotalAppointmentsToday = allAppointments.Count(a => a.Date == today),
            TotalAppointmentsThisWeek = allAppointments.Count(a => a.Date >= weekStart && a.Date <= today),
            PendingAppointments = allAppointments.Count(a => a.Status == AppointmentStatus.Pending),
            ConfirmedAppointments = allAppointments.Count(a => a.Status == AppointmentStatus.Confirmed),
            UrgentAppointments = allAppointments.Count(a => a.IsUrgent),
            TotalPatients = allPatients.Count(),
            TotalDoctors = allDoctors.Count(),
            ActiveBranches = allBranches.Count(b => b.IsActive),
            ActiveServices = allServices.Count(s => s.IsActive),
            UnresolvedContactMessages = allMessages.Count(m => m.Status != ContactMessageStatus.Closed),
            PendingModificationRequests =
                allServiceReqs.Count(r => r.Status == RequestStatus.Pending) +
                allFaqReqs.Count(r => r.Status == RequestStatus.Pending) +
                allOfferReqs.Count(r => r.Status == RequestStatus.Pending) +
                allBranchReqs.Count(r => r.Status == RequestStatus.Pending),
            TodayRevenue = completedAppts.Where(a => a.Date == today).Sum(a => a.Service!.Price),
            WeekRevenue = completedAppts.Where(a => a.Date >= weekStart).Sum(a => a.Service!.Price),
            TotalRevenue = completedAppts.Sum(a => a.Service!.Price)
        };
    }

    public async Task<DailyReportDto> GetDailyReportAsync(DateOnly date, CancellationToken ct = default)
    {
        var appointments = (await _uow.Appointments.GetByDateAsync(date, ct)).ToList();

        return new DailyReportDto
        {
            Date = date,
            TotalAppointments = appointments.Count,
            Attended = appointments.Count(a => a.AttendanceStatus == AttendanceStatus.Attended),
            Absent = appointments.Count(a => a.AttendanceStatus == AttendanceStatus.Absent),
            Cancelled = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
            Rejected = appointments.Count(a => a.Status == AppointmentStatus.Rejected),
            Completed = appointments.Count(a => a.Status == AppointmentStatus.Completed),
            UrgentCases = appointments.Count(a => a.IsUrgent),
            Revenue = appointments
                .Where(a => a.Status == AppointmentStatus.Completed && a.Service != null)
                .Sum(a => a.Service!.Price),
            Appointments = appointments.Select(a => new AppointmentSummaryExportDto
            {
                AppointmentId = a.Id,
                PatientName = a.Patient?.FullName ?? string.Empty,
                DoctorName = a.Doctor?.FullName ?? string.Empty,
                BranchName = a.Branch?.Name ?? string.Empty,
                ServiceName = a.Service?.Name ?? string.Empty,
                StartTime = a.StartTime,
                Status = a.Status.ToString(),
                IsUrgent = a.IsUrgent
            }).ToList()
        };
    }

    public async Task<WeeklyReportDto> GetWeeklyReportAsync(DateOnly weekStart, CancellationToken ct = default)
    {
        var weekEnd = weekStart.AddDays(6);
        var allAppointments = await _uow.Appointments.GetAllAsync(ct);
        var weeklyAppointments = allAppointments.Where(a => a.Date >= weekStart && a.Date <= weekEnd).ToList();

        var dailyBreakdown = new List<DailyReportDto>();
        for (var d = weekStart; d <= weekEnd; d = d.AddDays(1))
        {
            var dayAppointments = weeklyAppointments.Where(a => a.Date == d).ToList();
            dailyBreakdown.Add(new DailyReportDto
            {
                Date = d,
                TotalAppointments = dayAppointments.Count,
                Attended = dayAppointments.Count(a => a.AttendanceStatus == AttendanceStatus.Attended),
                Absent = dayAppointments.Count(a => a.AttendanceStatus == AttendanceStatus.Absent),
                Cancelled = dayAppointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                Rejected = dayAppointments.Count(a => a.Status == AppointmentStatus.Rejected),
                Completed = dayAppointments.Count(a => a.Status == AppointmentStatus.Completed),
                UrgentCases = dayAppointments.Count(a => a.IsUrgent),
                Revenue = dayAppointments
                    .Where(a => a.Status == AppointmentStatus.Completed && a.Service != null)
                    .Sum(a => a.Service!.Price),
                Appointments = dayAppointments.Select(a => new AppointmentSummaryExportDto
                {
                    AppointmentId = a.Id,
                    PatientName = a.Patient?.FullName ?? string.Empty,
                    DoctorName = a.Doctor?.FullName ?? string.Empty,
                    BranchName = a.Branch?.Name ?? string.Empty,
                    ServiceName = a.Service?.Name ?? string.Empty,
                    StartTime = a.StartTime,
                    Status = a.Status.ToString(),
                    IsUrgent = a.IsUrgent
                }).ToList()
            });
        }

        return new WeeklyReportDto
        {
            WeekStart = weekStart,
            WeekEnd = weekEnd,
            TotalAppointments = weeklyAppointments.Count,
            Attended = weeklyAppointments.Count(a => a.AttendanceStatus == AttendanceStatus.Attended),
            Absent = weeklyAppointments.Count(a => a.AttendanceStatus == AttendanceStatus.Absent),
            Cancelled = weeklyAppointments.Count(a => a.Status == AppointmentStatus.Cancelled),
            WeekRevenue = weeklyAppointments
                .Where(a => a.Status == AppointmentStatus.Completed && a.Service != null)
                .Sum(a => a.Service!.Price),
            DailyBreakdown = dailyBreakdown
        };
    }

    public async Task<byte[]> ExportDailyReportAsCsvAsync(DateOnly date, CancellationToken ct = default)
    {
        var report = await GetDailyReportAsync(date, ct);
        return BuildCsv(report.Appointments);
    }

    public async Task<byte[]> ExportWeeklyReportAsCsvAsync(DateOnly weekStart, CancellationToken ct = default)
    {
        var report = await GetWeeklyReportAsync(weekStart, ct);
        var all = report.DailyBreakdown.SelectMany(d => d.Appointments).ToList();
        return BuildCsv(all);
    }

    private static byte[] BuildCsv(List<AppointmentSummaryExportDto> appointments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("AppointmentId,PatientName,DoctorName,BranchName,ServiceName,StartTime,Status,IsUrgent");
        foreach (var a in appointments)
            sb.AppendLine($"{a.AppointmentId},{Esc(a.PatientName)},{Esc(a.DoctorName)},{Esc(a.BranchName)},{Esc(a.ServiceName)},{a.StartTime},{a.Status},{a.IsUrgent}");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Esc(string v) => v.Contains(',') ? $"\"{v}\"" : v;
}
