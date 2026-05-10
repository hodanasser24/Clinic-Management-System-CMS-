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
        var charts = await _uow.DentalCharts.FindAsync(dc => dc.PatientId == patientId, ct);
        var chart = charts.FirstOrDefault();
        if (chart == null) throw new NotFoundException($"Dental chart for patient {patientId} not found.");
        return await MapToResponseAsync(chart, ct);
    }

    public async Task<DentalChartResponseDto> UpdateChartNotesAsync(int patientId, UpdateDentalChartRequestDto dto, int requestingDoctorId, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(requestingDoctorId, ct);
        if (doctor == null) throw new ForbiddenException("Only doctors can update dental charts.");

        var charts = await _uow.DentalCharts.FindAsync(dc => dc.PatientId == patientId, ct);
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

        var charts = await _uow.DentalCharts.FindAsync(dc => dc.PatientId == patientId, ct);
        var chart = charts.FirstOrDefault();

        if (chart == null)
        {
            chart = new DentalChart { PatientId = patientId, LastUpdated = DateTime.UtcNow };
            await _uow.DentalCharts.AddAsync(chart, ct);
            await _uow.SaveChangesAsync(ct);
        }

        // BR-44: unique (ChartId, ToothNumber) — upsert
        var toothRecords = await _uow.ToothRecords.FindAsync(t => t.ChartId == chart.Id && t.ToothNumber == dto.ToothNumber, ct);
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

    private async Task<DentalChartResponseDto> MapToResponseAsync(DentalChart dc, CancellationToken ct)
    {
        var patient = dc.Patient ?? await _uow.Patients.GetByIdAsync(dc.PatientId, ct);
        var records = dc.ToothRecords?.ToList()
            ?? (await _uow.ToothRecords.FindAsync(t => t.ChartId == dc.Id, ct)).ToList();

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
}
