using DCMS.Application.DTOs.Prescriptions;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces;
using System.Text;

namespace DCMS.Application.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly IUnitOfWork _uow;

    public PrescriptionService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PrescriptionResponseDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var prescription = await _uow.Prescriptions.GetByIdAsync(id, ct);
        if (prescription == null) throw new NotFoundException($"Prescription {id} not found.");
        return MapToResponse(prescription);
    }

    public async Task<PrescriptionResponseDto> GetByReportIdAsync(int reportId, CancellationToken ct = default)
    {
        var prescriptions = await _uow.Prescriptions.FindAsync(p => p.ReportId == reportId, ct);
        var prescription = prescriptions.FirstOrDefault();
        if (prescription == null) throw new NotFoundException($"No prescription found for report {reportId}.");
        return MapToResponse(prescription);
    }

    public async Task<PrescriptionResponseDto> CreateAsync(CreatePrescriptionRequestDto dto, CancellationToken ct = default)
    {
        // BR-39: Prescription requires existing Report
        var report = await _uow.Reports.GetByIdAsync(dto.ReportId, ct);
        if (report == null)
            throw new NotFoundException("Report not found. A prescription must be linked to an existing report.");

        var existing = await _uow.Prescriptions.FindAsync(p => p.ReportId == dto.ReportId, ct);
        if (existing.Any())
            throw new ConflictException("A prescription already exists for this report.");

        // BR-18: Must have items
        if (dto.Items == null || !dto.Items.Any())
            throw new BusinessRuleException("Prescription must contain at least one item.");

        var prescription = new Prescription
        {
            ReportId = dto.ReportId,
            GeneralInstructions = dto.GeneralInstructions,
            Items = dto.Items.Select(i => new PrescriptionItem
            {
                MedicationName = i.MedicationName,
                Dosage = i.Dosage,
                Frequency = i.Frequency,
                Duration = i.Duration,
                Route = i.Route,
                Notes = i.Notes
            }).ToList()
        };

        await _uow.Prescriptions.AddAsync(prescription, ct);
        await _uow.SaveChangesAsync(ct);

        return MapToResponse(prescription);
    }

    private static PrescriptionResponseDto MapToResponse(Prescription p) => new()
    {
        Id = p.Id,
        ReportId = p.ReportId,
        GeneralInstructions = p.GeneralInstructions,
        Items = p.Items?.Select(i => new PrescriptionItemResponseDto
        {
            Id = i.Id,
            PrescriptionId = i.PrescriptionId,
            MedicationName = i.MedicationName,
            Dosage = i.Dosage,
            Frequency = i.Frequency,
            Duration = i.Duration,
            Route = i.Route,
            Notes = i.Notes
        }).ToList() ?? new(),
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    // ── Update ────────────────────────────────────────────────────

    public async Task<PrescriptionResponseDto> UpdateAsync(
        int id, int requestingDoctorId, CreatePrescriptionRequestDto dto, CancellationToken ct = default)
    {
        var prescription = await _uow.Prescriptions.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Prescription {id} not found.");

        var report = await _uow.Reports.GetByIdAsync(prescription.ReportId, ct);
        if (report?.DoctorId != requestingDoctorId)
            throw new ForbiddenException("Only the prescribing doctor can update this prescription.");

        prescription.GeneralInstructions = dto.GeneralInstructions;

        // Replace items
        prescription.Items.Clear();
        foreach (var item in dto.Items ?? [])
        {
            prescription.Items.Add(new PrescriptionItem
            {
                PrescriptionId = prescription.Id,
                MedicationName = item.MedicationName,
                Dosage         = item.Dosage,
                Frequency      = item.Frequency,
                Duration       = item.Duration,
                Route          = item.Route,
                Notes          = item.Notes
            });
        }

        await _uow.SaveChangesAsync(ct);
        return MapToResponse(prescription);
    }

    // ── PDF Export ────────────────────────────────────────────────

    /// <summary>
    /// Generates a text-based prescription export.
    /// In production, swap StringBuilder for QuestPDF or iTextSharp.
    /// </summary>
    public async Task<byte[]> ExportPdfAsync(int id, CancellationToken ct = default)
    {
        var p = await _uow.Prescriptions.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Prescription {id} not found.");

        var sb = new StringBuilder();
        sb.AppendLine("===========================================");
        sb.AppendLine("      DENTAL CLINIC MANAGEMENT SYSTEM      ");
        sb.AppendLine("              PRESCRIPTION                 ");
        sb.AppendLine("===========================================");
        sb.AppendLine();
        sb.AppendLine($"Prescription #: {p.Id}");
        sb.AppendLine($"Date          : {p.CreatedAt:dd MMM yyyy}");
        sb.AppendLine();
        sb.AppendLine($"Report #: {p.ReportId}");
        sb.AppendLine();
        sb.AppendLine("-------------------------------------------");
        sb.AppendLine("MEDICATIONS:");
        sb.AppendLine("-------------------------------------------");

        int i = 1;
        foreach (var item in p.Items)
        {
            sb.AppendLine($"{i++}. {item.MedicationName}");
            sb.AppendLine($"   Dosage    : {item.Dosage}");
            sb.AppendLine($"   Frequency : {item.Frequency}");
            if (!string.IsNullOrEmpty(item.Duration))      sb.AppendLine($"   Duration  : {item.Duration}");
            sb.AppendLine($"   Route     : {item.Route}");
            if (!string.IsNullOrEmpty(item.Notes))         sb.AppendLine($"   Notes     : {item.Notes}");
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(p.GeneralInstructions))
        {
            sb.AppendLine("-------------------------------------------");
            sb.AppendLine($"General Instructions: {p.GeneralInstructions}");
        }

        sb.AppendLine("===========================================");
        sb.AppendLine("Doctor's Signature: ____________________");
        sb.AppendLine("===========================================");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
