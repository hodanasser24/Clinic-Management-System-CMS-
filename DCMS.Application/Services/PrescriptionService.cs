using DCMS.Application.DTOs.Prescriptions;
using DCMS.Application.DTOs.Common;
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
        var prescription = await _uow.Prescriptions.GetByIdWithItemsAsync(id, ct);
        if (prescription == null) throw new NotFoundException($"Prescription {id} not found.");
        
        // Ensure Report is loaded for MapToResponse
        if (prescription.Report == null)
            prescription.Report = await _uow.Reports.GetByIdAsync(prescription.ReportId, ct);
            
        return MapToResponse(prescription);
    }

    public async Task<PrescriptionResponseDto> GetByReportIdAsync(int reportId, CancellationToken ct = default)
    {
        var prescription = await _uow.Prescriptions.GetByReportIdWithItemsAsync(reportId, ct);
        if (prescription == null) throw new NotFoundException($"No prescription found for report {reportId}.");
        
        if (prescription.Report == null)
            prescription.Report = await _uow.Reports.GetByIdAsync(prescription.ReportId, ct);
            
        return MapToResponse(prescription);
    }

    public async Task<PagedResultDto<PrescriptionResponseDto>> GetByPatientAsync(int patientId, int page, int pageSize, CancellationToken ct = default)
    {
        var pagedResult = await _uow.Prescriptions.GetByPatientWithDetailsAsync(patientId, page, pageSize, ct);
        return new PagedResultDto<PrescriptionResponseDto>
        {
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            Items = pagedResult.Items.Select(MapToResponse).ToList()
        };
    }

    public async Task<PrescriptionResponseDto> CreateAsync(
        CreatePrescriptionRequestDto dto, int requestingDoctorId, CancellationToken ct = default)
    {
        // BR-39: Prescription requires existing Report
        var report = await _uow.Reports.GetByIdAsync(dto.ReportId, ct);
        if (report == null)
            throw new NotFoundException("Report not found. A prescription must be linked to an existing report.");

        if (report.DoctorId != requestingDoctorId)
            throw new ForbiddenException("Only the report's author can prescribe medication for it.");

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
        PatientId = p.Report?.PatientId ?? 0,
        DoctorId = p.Report?.DoctorId ?? 0,
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
        var prescription = await _uow.Prescriptions.GetByIdWithItemsAsync(id, ct)
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

    public async Task<byte[]> ExportPdfAsync(int id, CancellationToken ct = default)
    {
        var p = await _uow.Prescriptions.GetByIdWithItemsAsync(id, ct)
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

        return BuildPdf(sb.ToString());
    }

    private static byte[] BuildPdf(string text)
    {
        const int pageWidth = 612;
        const int pageHeight = 792;
        var lines = text.Replace("\r", string.Empty).Split('\n').Take(46)
            .Select(line => EscapePdfText(line.Length > 95 ? line[..95] : line));
        var content = new StringBuilder("BT\n/F1 10 Tf\n50 750 Td\n14 TL\n");
        foreach (var line in lines)
            content.Append('(').Append(line).Append(") Tj\nT*\n");
        content.Append("ET");
        var contentBytes = Encoding.ASCII.GetBytes(content.ToString());
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {pageWidth} {pageHeight}] /Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>",
            $"<< /Length {contentBytes.Length} >>\nstream\n{Encoding.ASCII.GetString(contentBytes)}\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("%PDF-1.4\n");
        writer.Flush();
        var offsets = new List<long> { 0 };
        for (var index = 0; index < objects.Length; index++)
        {
            offsets.Add(stream.Position);
            writer.Write($"{index + 1} 0 obj\n{objects[index]}\nendobj\n");
            writer.Flush();
        }
        var xrefOffset = stream.Position;
        writer.Write($"xref\n0 {objects.Length + 1}\n");
        writer.Write("0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1)) writer.Write($"{offset:D10} 00000 n \n");
        writer.Write($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
        writer.Flush();
        return stream.ToArray();
    }

    private static string EscapePdfText(string value) => new(value
        .Select(character => character is '\\' or '(' or ')' ? $"\\{character}" : character <= 127 ? character.ToString() : "?")
        .SelectMany(part => part)
        .ToArray());
}
