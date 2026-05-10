using DCMS.Domain.Enums;

namespace DCMS.Application.DTOs.DentalChart;

public class UpdateDentalChartRequestDto
{
    public string? Notes { get; set; }
}

public class UpsertToothRecordRequestDto
{
    public int ToothNumber { get; set; }
    public ToothStatus ToothStatus { get; set; }
    public TreatmentType? TreatmentType { get; set; }
    public DateOnly? TreatmentDate { get; set; }
    public string? Notes { get; set; }
    public int? LastUpdatedInReportId { get; set; }
}

public class DentalChartResponseDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<ToothRecordResponseDto> ToothRecords { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ToothRecordResponseDto
{
    public int Id { get; set; }
    public int ChartId { get; set; }
    public int ToothNumber { get; set; }
    public ToothStatus ToothStatus { get; set; }
    public TreatmentType? TreatmentType { get; set; }
    public DateOnly? TreatmentDate { get; set; }
    public string? Notes { get; set; }
    public int? LastUpdatedInReportId { get; set; }
}
