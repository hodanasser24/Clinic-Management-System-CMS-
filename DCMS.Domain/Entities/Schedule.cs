using System;
using DCMS.Domain.Common;

namespace DCMS.Domain.Entities;

public class Schedule : BaseEntity
{
    public int DoctorId { get; set; }
    public int BranchId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public int SessionDurationMinutes { get; set; }
    public int? BreakDurationMinutes { get; set; }

    // Navigation Properties
    public virtual Doctor Doctor { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
}
