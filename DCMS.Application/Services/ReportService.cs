using DCMS.Application.DTOs.Reports;
using DCMS.Application.DTOs.Common;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;
using DCMS.Domain.Interfaces;

using AutoMapper;

namespace DCMS.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork            _uow;
    private readonly INotificationService   _notificationService;
    private readonly IMapper                _mapper;

    public ReportService(IUnitOfWork uow, INotificationService notificationService, IMapper mapper)
    {
        _uow                 = uow;
        _notificationService = notificationService;
        _mapper              = mapper;
    }

    public async Task<ReportResponseDto> GetByIdAsync(int id, UserRole callerRole, CancellationToken ct = default)
    {
        var report = await _uow.Reports.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException($"Report {id} not found.");
        return MapToResponse(report, callerRole);
    }

    public async Task<PagedResultDto<ReportResponseDto>> GetByPatientAsync(
        int patientId, UserRole callerRole, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Reports.GetByPatientWithDetailsAsync(patientId, page, pageSize, ct);
        return new PagedResultDto<ReportResponseDto>
        {
            Items      = paged.Items.Select(r => MapToResponse(r, callerRole)).ToList(),
            TotalCount = paged.TotalCount,
            Page       = paged.Page,
            PageSize   = paged.PageSize
        };
    }

    public async Task<PagedResultDto<ReportResponseDto>> GetByDoctorAsync(
        int doctorId, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Reports.GetByDoctorWithDetailsAsync(doctorId, page, pageSize, ct);
        return new PagedResultDto<ReportResponseDto>
        {
            Items      = paged.Items.Select(r => MapToResponse(r, UserRole.Doctor)).ToList(),
            TotalCount = paged.TotalCount,
            Page       = paged.Page,
            PageSize   = paged.PageSize
        };
    }

    public async Task<ReportResponseDto> CreateAsync(CreateReportRequestDto dto, CancellationToken ct = default)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(dto.AppointmentId, ct)
            ?? throw new NotFoundException("Appointment not found.");

        if (appointment.Status != AppointmentStatus.Completed)
            throw new BusinessRuleException("Reports can only be created for completed appointments.");

        var existing = await _uow.Reports.GetByAppointmentIdAsync(dto.AppointmentId, ct);
        if (existing != null)
            throw new ConflictException("A report already exists for this appointment.");

        var report = new Report
        {
            AppointmentId       = dto.AppointmentId,
            PatientId           = dto.PatientId,
            DoctorId            = dto.DoctorId,
            Diagnosis           = dto.Diagnosis,
            Treatment           = dto.Treatment,
            Notes               = dto.Notes,
            InternalNotes       = dto.InternalNotes,
            CaseStatus          = dto.CaseStatus,
            FollowUpInstructions = dto.FollowUpInstructions,
            TreatmentPlan       = dto.TreatmentPlan,
            DietInstructions    = dto.DietInstructions,
            AllowedFood         = dto.AllowedFood,
            RestrictedFood      = dto.RestrictedFood,
            HomeCareInstructions = dto.HomeCareInstructions
        };

        await _uow.Reports.AddAsync(report, ct);
        await _uow.SaveChangesAsync(ct);

        await _notificationService.SendAsync(
            dto.PatientId, NotificationType.ReportCreated, NotificationPriority.Normal,
            "Medical Report Ready", "Your medical report has been created and is available for review.",
            report.Id, "Report", ct);

        // Reload with details for full response
        var full = await _uow.Reports.GetByIdWithDetailsAsync(report.Id, ct) ?? report;
        return MapToResponse(full, UserRole.Doctor);
    }

    public async Task<ReportResponseDto> UpdateAsync(
        int id, UpdateReportRequestDto dto, int requestingDoctorId, CancellationToken ct = default)
    {
        var report = await _uow.Reports.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException($"Report {id} not found.");

        if (report.DoctorId != requestingDoctorId)
            throw new ForbiddenException("Only the report's author can update it.");

        report.Diagnosis            = dto.Diagnosis;
        report.Treatment            = dto.Treatment;
        report.Notes                = dto.Notes;
        report.InternalNotes        = dto.InternalNotes;
        report.CaseStatus           = dto.CaseStatus;
        report.FollowUpInstructions = dto.FollowUpInstructions;
        report.TreatmentPlan        = dto.TreatmentPlan;
        report.DietInstructions     = dto.DietInstructions;
        report.AllowedFood          = dto.AllowedFood;
        report.RestrictedFood       = dto.RestrictedFood;
        report.HomeCareInstructions = dto.HomeCareInstructions;

        await _uow.SaveChangesAsync(ct);
        return MapToResponse(report, UserRole.Doctor);
    }

    // ── Report comparison ─────────────────────────────────────────────────────

    public async Task<ReportCompareResponseDto> CompareAsync(
        int reportId1, int reportId2, CancellationToken ct = default)
    {
        var r1 = await _uow.Reports.GetByIdWithDetailsAsync(reportId1, ct)
            ?? throw new NotFoundException($"Report {reportId1} not found.");
        var r2 = await _uow.Reports.GetByIdWithDetailsAsync(reportId2, ct)
            ?? throw new NotFoundException($"Report {reportId2} not found.");

        if (r1.PatientId != r2.PatientId)
            throw new BusinessRuleException("Cannot compare reports from different patients.");

        // Ensure r1 is earlier
        if (r1.CreatedAt > r2.CreatedAt) (r1, r2) = (r2, r1);

        // Collect tooth records for each report's associated dental chart snapshot
        var chart1teeth = r1.Patient?.DentalChart?.ToothRecords ?? [];
        var chart2teeth = r2.Patient?.DentalChart?.ToothRecords ?? [];

        var teeth1 = chart1teeth.ToDictionary(t => t.ToothNumber);
        var teeth2 = chart2teeth.ToDictionary(t => t.ToothNumber);
        var allTeeth = teeth1.Keys.Union(teeth2.Keys).OrderBy(n => n);

        var diffs = allTeeth.Select(tooth =>
        {
            var before = teeth1.GetValueOrDefault(tooth);
            var after  = teeth2.GetValueOrDefault(tooth);
            return new ToothDiffDto
            {
                ToothNumber  = tooth,
                BeforeStatus = before?.ToothStatus,
                AfterStatus  = after?.ToothStatus,
                BeforeLabel  = before?.ToothStatus.ToString(),
                AfterLabel   = after?.ToothStatus.ToString(),
                Changed      = before?.ToothStatus != after?.ToothStatus
            };
        }).ToList();

        return new ReportCompareResponseDto
        {
            EarlierReport          = MapToResponse(r1, UserRole.Doctor),
            LaterReport            = MapToResponse(r2, UserRole.Doctor),
            ToothDiffs             = diffs,
            ChangedCount           = diffs.Count(d => d.Changed),
            ImprovedCount          = diffs.Count(d => d.Changed && d.AfterStatus is
                ToothStatus.Healthy or ToothStatus.Filled or ToothStatus.CrownPlaced),
            WorsenedCount          = diffs.Count(d => d.Changed && d.AfterStatus is
                ToothStatus.Missing or ToothStatus.Decayed or ToothStatus.NeedsExtraction),
            DiagnosisChangeSummary = r1.Diagnosis == r2.Diagnosis
                ? "No change in primary diagnosis."
                : $"Diagnosis changed between reports #{r1.Id} and #{r2.Id}."
        };
    }

    // ── Mapping with BR-57 enforcement ────────────────────────────────────────

    /// <summary>
    /// BR-57: InternalNotes is ONLY included for Doctor and Owner roles.
    /// Admin receives AdminReportResponseDto (no InternalNotes, but AllowedFood/RestrictedFood).
    /// Patient receives the base ReportResponseDto.
    /// </summary>
    private ReportResponseDto MapToResponse(Report r, UserRole callerRole)
    {
        if (callerRole is UserRole.Doctor or UserRole.Owner)
        {
            return _mapper.Map<DoctorReportResponseDto>(r);
        }

        if (callerRole is UserRole.Admin)
        {
            // Admin sees clinical fields but NOT InternalNotes (BR-57)
            return _mapper.Map<AdminReportResponseDto>(r);
        }

        // Patient: base DTO only — no internal or dietary detail fields
        return _mapper.Map<ReportResponseDto>(r);
    }
}
