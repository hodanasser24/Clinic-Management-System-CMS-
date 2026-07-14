using System.Collections.Generic;

namespace DCMS.Application.DTOs.DentalChart;

public class BulkUpsertToothRecordsRequestDto
{
    public int AppointmentId { get; set; }
    public List<UpsertToothRecordRequestDto> Records { get; set; } = new();
}
