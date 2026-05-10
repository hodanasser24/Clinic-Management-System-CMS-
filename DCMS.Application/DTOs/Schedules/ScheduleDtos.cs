namespace DCMS.Application.DTOs.Schedules;

public class CreateScheduleRequestDto
{
    public int DoctorId { get; set; }
    public int BranchId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SessionDurationMinutes { get; set; }
}

public class UpdateScheduleRequestDto
{
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SessionDurationMinutes { get; set; }
    public bool IsActive { get; set; }
}

public class ScheduleResponseDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = null!;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SessionDurationMinutes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AvailableSlotDto
{
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

// SRS §4.2: Admin submits schedule change requests
public class CreateScheduleChangeRequestDto
{
    public int RequestingAdminId { get; set; }  // Admin who submits (was DoctorId — FIXED)
    public int OwnerId { get; set; }
    public int ScheduleId { get; set; }
    public DayOfWeek? ProposedDayOfWeek { get; set; }
    public TimeOnly? ProposedStartTime { get; set; }
    public TimeOnly? ProposedEndTime { get; set; }
    public int? ProposedSessionDurationMinutes { get; set; }
    public string? Reason { get; set; }
}

public class ScheduleChangeRequestResponseDto
{
    public int Id { get; set; }
    public int RequestingDoctorId { get; set; }
    public string RequestingDoctorName { get; set; } = null!;
    public int ScheduleId { get; set; }
    public DayOfWeek? ProposedDayOfWeek { get; set; }
    public TimeOnly? ProposedStartTime { get; set; }
    public TimeOnly? ProposedEndTime { get; set; }
    public int? ProposedSessionDurationMinutes { get; set; }
    public string? Reason { get; set; }
    public bool DoctorApproved { get; set; }
    public bool OwnerApproved { get; set; }
    public DCMS.Domain.Enums.RequestStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// BR-6/BR-7: Doctor submits a schedule change request.
/// Expires after 24h (BR-7) and requires Owner approval (BR-6).
/// </summary>
public class SubmitScheduleChangeRequestDto
{
    public DayOfWeek? NewDayOfWeek { get; set; }
    public TimeOnly?  NewStartTime { get; set; }
    public TimeOnly?  NewEndTime   { get; set; }
    public string?    Reason       { get; set; }
}
