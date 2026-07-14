using DCMS.Application.DTOs.DentalChart;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces;

namespace DCMS.Application.Services;

public class DentalChartService : IDentalChartService
{
    private readonly IUnitOfWork _uow;

    public DentalChartService(IUnitOfWork uow) => _uow = uow;

    public async Task<DentalChartResponseDto> GetByPatientIdAsync(int patientId, CancellationToken ct = default)
    {
        var charts = await _uow.DentalCharts.FindTrackedAsync(dc => dc.PatientId == patientId, ct);
        var chart = charts.FirstOrDefault();
        if (chart == null)
        {
            var patient = await _uow.Patients.GetByIdAsync(patientId, ct);
            return new DentalChartResponseDto
            {
                Id = 0,
                PatientId = patientId,
                PatientName = patient?.FullName ?? string.Empty,
                Notes = string.Empty,
                LastUpdated = DateTime.UtcNow,
                ToothRecords = new List<ToothRecordResponseDto>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        return await MapToResponseAsync(chart, ct);
    }

    public async Task<DentalChartResponseDto> UpdateChartNotesAsync(int patientId, UpdateDentalChartRequestDto dto, int requestingDoctorId, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(requestingDoctorId, ct);
        if (doctor == null) throw new ForbiddenException("Only doctors can update dental charts.");

        var appointment = await _uow.Appointments.GetByIdAsync(dto.AppointmentId, ct)
            ?? throw new NotFoundException("Appointment not found.");
            
        if (appointment.PatientId != patientId || appointment.DoctorId != requestingDoctorId)
            throw new ForbiddenException("You are not authorized to edit this dental chart for this appointment.");
            
        if (appointment.Status != DCMS.Domain.Enums.AppointmentStatus.Confirmed)
            throw new BusinessRuleException("Dental charts can only be edited during an active confirmed appointment.");

        var charts = await _uow.DentalCharts.FindTrackedAsync(dc => dc.PatientId == patientId, ct);
        var chart = charts.FirstOrDefault();

        if (chart == null)
        {
            chart = new DentalChart { PatientId = patientId, Notes = dto.Notes, LastUpdated = DateTime.UtcNow };
            await _uow.DentalCharts.AddAsync(chart, ct);
        }
        else
        {
            chart.Notes = dto.Notes;
            chart.LastUpdated = DateTime.UtcNow;
        }

        await _uow.SaveChangesAsync(ct);
        return await MapToResponseAsync(chart, ct);
    }

    public async Task<DentalChartResponseDto> UpsertToothRecordAsync(int patientId, UpsertToothRecordRequestDto dto, int requestingDoctorId, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(requestingDoctorId, ct);
        if (doctor == null) throw new ForbiddenException("Only doctors can update tooth records.");
        
        var appointment = await _uow.Appointments.GetByIdAsync(dto.AppointmentId, ct)
            ?? throw new NotFoundException("Appointment not found.");
            
        if (appointment.PatientId != patientId || appointment.DoctorId != requestingDoctorId)
            throw new ForbiddenException("You are not authorized to edit this dental chart for this appointment.");
            
        if (appointment.Status != DCMS.Domain.Enums.AppointmentStatus.Confirmed)
            throw new BusinessRuleException("Dental charts can only be edited during an active confirmed appointment.");
            
        await ValidateReportAssociationAsync(patientId, dto.LastUpdatedInReportId, requestingDoctorId, ct);

        var charts = await _uow.DentalCharts.FindTrackedAsync(dc => dc.PatientId == patientId, ct);
        var chart = charts.FirstOrDefault();

        if (chart == null)
        {
            chart = new DentalChart { PatientId = patientId, LastUpdated = DateTime.UtcNow };
            await _uow.DentalCharts.AddAsync(chart, ct);
            await _uow.SaveChangesAsync(ct);
        }

        // BR-44: unique (ChartId, ToothNumber) — upsert
        var toothRecords = await _uow.ToothRecords.FindTrackedAsync(t => t.ChartId == chart.Id && t.ToothNumber == dto.ToothNumber, ct);
        var toothRecord = toothRecords.FirstOrDefault();

        if (toothRecord == null)
        {
            toothRecord = new ToothRecord
            {
                ChartId = chart.Id,
                ToothNumber = dto.ToothNumber,
                ToothStatus = dto.ToothStatus,
                TreatmentType = dto.TreatmentType,
                TreatmentDate = dto.TreatmentDate,
                Notes = dto.Notes,
                LastUpdatedInReportId = dto.LastUpdatedInReportId,
                LastUpdated = DateTime.UtcNow
            };
            await _uow.ToothRecords.AddAsync(toothRecord, ct);
        }
        else
        {
            toothRecord.ToothStatus = dto.ToothStatus;
            toothRecord.TreatmentType = dto.TreatmentType;
            toothRecord.TreatmentDate = dto.TreatmentDate;
            toothRecord.Notes = dto.Notes;
            toothRecord.LastUpdatedInReportId = dto.LastUpdatedInReportId;
            toothRecord.LastUpdated = DateTime.UtcNow;
        }

        chart.LastUpdated = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);

        var updatedCharts = await _uow.DentalCharts.FindAsync(dc => dc.PatientId == patientId, ct);
        return await MapToResponseAsync(updatedCharts.First(), ct);
    }

    public async Task<DentalChartResponseDto> BulkUpsertToothRecordsAsync(int patientId, BulkUpsertToothRecordsRequestDto dto, int requestingDoctorId, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(requestingDoctorId, ct);
        if (doctor == null) throw new ForbiddenException("Only doctors can update tooth records.");

        var appointment = await _uow.Appointments.GetByIdAsync(dto.AppointmentId, ct)
            ?? throw new NotFoundException("Appointment not found.");
            
        if (appointment.PatientId != patientId || appointment.DoctorId != requestingDoctorId)
            throw new ForbiddenException("You are not authorized to edit this dental chart for this appointment.");
            
        if (appointment.Status != DCMS.Domain.Enums.AppointmentStatus.Confirmed)
            throw new BusinessRuleException("Dental charts can only be edited during an active confirmed appointment.");

        foreach (var record in dto.Records)
            await ValidateReportAssociationAsync(patientId, record.LastUpdatedInReportId, requestingDoctorId, ct);

        var charts = await _uow.DentalCharts.FindTrackedAsync(dc => dc.PatientId == patientId, ct);
        var chart = charts.FirstOrDefault();

        if (chart == null)
        {
            chart = new DentalChart { PatientId = patientId, LastUpdated = DateTime.UtcNow };
            await _uow.DentalCharts.AddAsync(chart, ct);
            await _uow.SaveChangesAsync(ct);
        }

        var existingRecords = (await _uow.ToothRecords.FindTrackedAsync(t => t.ChartId == chart.Id, ct)).ToList();

        foreach (var recordDto in dto.Records)
        {
            var toothRecord = existingRecords.FirstOrDefault(t => t.ToothNumber == recordDto.ToothNumber);

            if (toothRecord == null)
            {
                toothRecord = new ToothRecord
                {
                    ChartId = chart.Id,
                    ToothNumber = recordDto.ToothNumber,
                    ToothStatus = recordDto.ToothStatus,
                    TreatmentType = recordDto.TreatmentType,
                    TreatmentDate = recordDto.TreatmentDate,
                    Notes = recordDto.Notes,
                    LastUpdatedInReportId = recordDto.LastUpdatedInReportId,
                    LastUpdated = DateTime.UtcNow
                };
                await _uow.ToothRecords.AddAsync(toothRecord, ct);
            }
            else
            {
                toothRecord.ToothStatus = recordDto.ToothStatus;
                toothRecord.TreatmentType = recordDto.TreatmentType;
                toothRecord.TreatmentDate = recordDto.TreatmentDate;
                toothRecord.Notes = recordDto.Notes;
                toothRecord.LastUpdatedInReportId = recordDto.LastUpdatedInReportId;
                toothRecord.LastUpdated = DateTime.UtcNow;
            }
        }

        chart.LastUpdated = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);

        var updatedCharts = await _uow.DentalCharts.FindAsync(dc => dc.PatientId == patientId, ct);
        return await MapToResponseAsync(updatedCharts.First(), ct);
    }

    private async Task<DentalChartResponseDto> MapToResponseAsync(DentalChart dc, CancellationToken ct)
    {
        var patient = dc.Patient ?? await _uow.Patients.GetByIdAsync(dc.PatientId, ct);
        var records = (await _uow.ToothRecords.FindAsync(t => t.ChartId == dc.Id, ct)).ToList();

        return new DentalChartResponseDto
        {
            Id = dc.Id,
            PatientId = dc.PatientId,
            PatientName = patient?.FullName ?? string.Empty,
            Notes = dc.Notes,
            LastUpdated = dc.LastUpdated,
            ToothRecords = records.Select(t => new ToothRecordResponseDto
            {
                Id = t.Id,
                ChartId = t.ChartId,
                ToothNumber = t.ToothNumber,
                ToothStatus = t.ToothStatus,
                TreatmentType = t.TreatmentType,
                TreatmentDate = t.TreatmentDate,
                Notes = t.Notes,
                LastUpdatedInReportId = t.LastUpdatedInReportId
            }).ToList(),
            CreatedAt = dc.CreatedAt,
            UpdatedAt = dc.UpdatedAt
        };
    }

    private async Task ValidateReportAssociationAsync(
        int patientId, int? reportId, int requestingDoctorId, CancellationToken ct)
    {
        if (!reportId.HasValue) return;

        var report = await _uow.Reports.GetByIdAsync(reportId.Value, ct)
            ?? throw new NotFoundException("Associated medical report not found.");

        if (report.PatientId != patientId)
            throw new BusinessRuleException("The associated report belongs to a different patient.");

        if (report.DoctorId != requestingDoctorId)
            throw new ForbiddenException("Only the report's author can associate it with tooth records.");
    }

    public async Task<byte[]> ExportPdfAsync(int patientId, int requestingDoctorId, CancellationToken ct = default)
    {
        var chartDto = await GetByPatientIdAsync(patientId, ct);
        
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("===========================================");
        sb.AppendLine("      DENTAL CLINIC MANAGEMENT SYSTEM      ");
        sb.AppendLine("              DENTAL CHART                 ");
        sb.AppendLine("===========================================");
        sb.AppendLine();
        sb.AppendLine($"Patient ID    : {chartDto.PatientId}");
        sb.AppendLine($"Patient Name  : {chartDto.PatientName}");
        sb.AppendLine($"Last Updated  : {chartDto.LastUpdated:dd MMM yyyy HH:mm}");
        sb.AppendLine();
        
        if (!string.IsNullOrEmpty(chartDto.Notes))
        {
            sb.AppendLine("-------------------------------------------");
            sb.AppendLine($"General Notes: {chartDto.Notes}");
        }

        sb.AppendLine("-------------------------------------------");
        sb.AppendLine("TOOTH RECORDS:");
        sb.AppendLine("-------------------------------------------");

        if (chartDto.ToothRecords.Any())
        {
            foreach (var item in chartDto.ToothRecords.OrderBy(t => t.ToothNumber))
            {
                sb.AppendLine($"Tooth # {item.ToothNumber} - Status: {item.ToothStatus}");
                if (item.TreatmentType != null) sb.AppendLine($"   Treatment : {item.TreatmentType}");
                if (item.TreatmentDate != null) sb.AppendLine($"   Date      : {item.TreatmentDate.Value:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(item.Notes)) sb.AppendLine($"   Notes     : {item.Notes}");
                sb.AppendLine();
            }
        }
        else
        {
            sb.AppendLine("No tooth records found.");
        }

        sb.AppendLine("===========================================");

        return BuildPdf(sb.ToString());
    }

    private static byte[] BuildPdf(string text)
    {
        const int pageWidth = 612;
        const int pageHeight = 792;
        var lines = text.Replace("\r", string.Empty).Split('\n').Take(46)
            .Select(line => EscapePdfText(line.Length > 95 ? line[..95] : line));
        var content = new System.Text.StringBuilder("BT\n/F1 10 Tf\n50 750 Td\n14 TL\n");
        foreach (var line in lines)
            content.Append('(').Append(line).Append(") Tj\nT*\n");
        content.Append("ET");
        var contentBytes = System.Text.Encoding.ASCII.GetBytes(content.ToString());
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {pageWidth} {pageHeight}] /Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>",
            $"<< /Length {contentBytes.Length} >>\nstream\n{System.Text.Encoding.ASCII.GetString(contentBytes)}\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };
        using var stream = new System.IO.MemoryStream();
        using var writer = new System.IO.StreamWriter(stream, System.Text.Encoding.ASCII, leaveOpen: true);
        writer.Write("%PDF-1.4\n");
        writer.Flush();
        var offsets = new System.Collections.Generic.List<long> { 0 };
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
