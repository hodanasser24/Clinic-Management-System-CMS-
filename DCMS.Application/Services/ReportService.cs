using DCMS.Application.DTOs.Reports;
using DCMS.Application.DTOs.Common;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;

namespace DCMS.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;

    public ReportService(IUnitOfWork uow, INotificationService notificationService)
    {
        _uow = uow;
        _notificationService = notificationService;
    }

    public async Task<ReportResponseDto> GetByIdAsync(int id, UserRole callerRole, CancellationToken ct = default)
    {
        var report = await _uow.Reports.GetByIdAsync(id, ct);
        if (report == null) throw new NotFoundException($"Report {id} not found.");
        return MapToResponse(report, callerRole);
    }

    public async Task<PagedResultDto<ReportResponseDto>> GetByPatientAsync(int patientId, UserRole callerRole, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Reports.GetByPatientAsync(patientId, page, pageSize, ct);
        return new PagedResultDto<ReportResponseDto>
        {
            Items = paged.Items.Select(r => MapToResponse(r, callerRole)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<PagedResultDto<ReportResponseDto>> GetByDoctorAsync(int doctorId, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Reports.GetByDoctorAsync(doctorId, page, pageSize, ct);
        return new PagedResultDto<ReportResponseDto>
        {
            Items = paged.Items.Select(r => MapToResponse(r, UserRole.Doctor)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<ReportResponseDto> CreateAsync(CreateReportRequestDto dto, CancellationToken ct = default)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(dto.AppointmentId, ct);
        if (appointment == null) throw new NotFoundException("Appointment not found.");

        if (appointment.Status != AppointmentStatus.Completed)
            throw new BusinessRuleException("Reports can only be created for completed appointments.");

        var existing = await _uow.Reports.GetByAppointmentIdAsync(dto.AppointmentId, ct);
        if (existing != null)
            throw new ConflictException("A report already exists for this appointment.");

        var report = new Report
        {
            AppointmentId = dto.AppointmentId,
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            Diagnosis = dto.Diagnosis,
            Treatment = dto.Treatment,
            InternalNotes = dto.InternalNotes,
            CaseStatus = dto.CaseStatus,
            FollowUpInstructions = dto.FollowUpInstructions,
            TreatmentPlan = dto.TreatmentPlan,
            DietInstructions = dto.DietInstructions,
            AllowedFood = dto.AllowedFood,
            RestrictedFood = dto.RestrictedFood,
            HomeCareInstructions = dto.HomeCareInstructions
        };

        await _uow.Reports.AddAsync(report, ct);
        await _uow.SaveChangesAsync(ct);

        // Notify patient that their report is ready
        await _notificationService.SendAsync(
            dto.PatientId, NotificationType.ReportCreated, NotificationPriority.Normal,
            "Medical Report Ready", "Your medical report has been created and is available for review.",
            report.Id, "Report", ct);

        return MapToResponse(report, UserRole.Doctor);
    }

    public async Task<ReportResponseDto> UpdateAsync(int id, UpdateReportRequestDto dto, int requestingDoctorId, CancellationToken ct = default)
    {
        var report = await _uow.Reports.GetByIdAsync(id, ct);
        if (report == null) throw new NotFoundException($"Report {id} not found.");

        if (report.DoctorId != requestingDoctorId)
            throw new ForbiddenException("Only the report's author can update it.");

        report.Diagnosis = dto.Diagnosis;
        report.Treatment = dto.Treatment;
        report.InternalNotes = dto.InternalNotes;
        report.CaseStatus = dto.CaseStatus;
        report.FollowUpInstructions = dto.FollowUpInstructions;
        report.TreatmentPlan = dto.TreatmentPlan;
        report.DietInstructions = dto.DietInstructions;
        report.AllowedFood = dto.AllowedFood;
        report.RestrictedFood = dto.RestrictedFood;
        report.HomeCareInstructions = dto.HomeCareInstructions;

        await _uow.SaveChangesAsync(ct);
        return MapToResponse(report, UserRole.Doctor);
    }

    // BR-57: InternalNotes only exposed to Doctor/Owner role
    private static ReportResponseDto MapToResponse(Report r, UserRole callerRole)
    {
        if (callerRole == UserRole.Doctor || callerRole == UserRole.Owner || callerRole == UserRole.Admin)
        {
            return new DoctorReportResponseDto
            {
                Id = r.Id,
                AppointmentId = r.AppointmentId,
                PatientId = r.PatientId,
                PatientName = r.Patient?.FullName ?? string.Empty,
                DoctorId = r.DoctorId,
                DoctorName = r.Doctor?.FullName ?? string.Empty,
                Diagnosis = r.Diagnosis,
                Treatment = r.Treatment,
                CaseStatus = r.CaseStatus,
                InternalNotes = r.InternalNotes,
                FollowUpInstructions = r.FollowUpInstructions,
                TreatmentPlan = r.TreatmentPlan,
                DietInstructions = r.DietInstructions,
                AllowedFood = r.AllowedFood,
                RestrictedFood = r.RestrictedFood,
                HomeCareInstructions = r.HomeCareInstructions,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            };
        }

        return new ReportResponseDto
        {
            Id = r.Id,
            AppointmentId = r.AppointmentId,
            PatientId = r.PatientId,
            PatientName = r.Patient?.FullName ?? string.Empty,
            DoctorId = r.DoctorId,
            DoctorName = r.Doctor?.FullName ?? string.Empty,
            Diagnosis = r.Diagnosis,
            Treatment = r.Treatment,
            CaseStatus = r.CaseStatus,
            FollowUpInstructions = r.FollowUpInstructions,
            TreatmentPlan = r.TreatmentPlan,
            DietInstructions = r.DietInstructions,
            HomeCareInstructions = r.HomeCareInstructions,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }

    // ── Compare (timeline diff) ───────────────────────────────────────────────

    /// <summary>
    /// Compare two reports for the same patient – returns a timeline diff of tooth statuses.
    /// </summary>
    public async Task<ReportCompareResponseDto> CompareAsync(
        int reportId1, int reportId2, CancellationToken ct = default)
    {
        var r1 = await _uow.Reports.GetByIdAsync(reportId1, ct)
            ?? throw new NotFoundException($"Report {reportId1} not found.");
        var r2 = await _uow.Reports.GetByIdAsync(reportId2, ct)
            ?? throw new NotFoundException($"Report {reportId2} not found.");

        if (r1.PatientId != r2.PatientId)
            throw new InvalidOperationException("Cannot compare reports from different patients.");

        // Ensure r1 is the earlier report
        if (r1.CreatedAt > r2.CreatedAt) (r1, r2) = (r2, r1);

        // Build tooth diff (dental chart evolution)
        var teeth1 = (r1.DentalChart?.ToothRecords ?? []).ToDictionary(t => t.ToothNumber);
        var teeth2 = (r2.DentalChart?.ToothRecords ?? []).ToDictionary(t => t.ToothNumber);
        var allTeeth = teeth1.Keys.Union(teeth2.Keys).OrderBy(n => n);

        var diffs = allTeeth.Select(tooth =>
        {
            var before = teeth1.GetValueOrDefault(tooth);
            var after  = teeth2.GetValueOrDefault(tooth);
            return new ToothDiffDto
            {
                ToothNumber  = tooth,
                BeforeStatus = before?.Status,
                AfterStatus  = after?.Status,
                BeforeLabel  = before?.Status.ToString(),
                AfterLabel   = after?.Status.ToString(),
                Changed      = before?.Status != after?.Status
            };
        }).ToList();

        return new ReportCompareResponseDto
        {
            EarlierReport = MapToResponse(r1, UserRole.Doctor),
            LaterReport   = MapToResponse(r2, UserRole.Doctor),
            ToothDiffs    = diffs,
            ChangedCount  = diffs.Count(d => d.Changed),
            ImprovedCount = diffs.Count(d =>
                d.Changed && d.AfterStatus is
                    ToothStatus.Healthy or ToothStatus.Filled or ToothStatus.Crowned),
            WorsendCount  = diffs.Count(d =>
                d.Changed && d.AfterStatus is
                    ToothStatus.Extracted or ToothStatus.Missing or ToothStatus.Decayed),
            DiagnosisChangeSummary = r1.Diagnosis == r2.Diagnosis
                ? "No change in diagnosis."
                : $"Diagnosis changed between report {r1.Id} and report {r2.Id}."
        };
    }
}
