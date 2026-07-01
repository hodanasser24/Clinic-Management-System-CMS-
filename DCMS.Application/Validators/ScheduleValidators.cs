using DCMS.Application.DTOs.Schedules;
using FluentValidation;

namespace DCMS.Application.Validators;

public class CreateScheduleRequestValidator : AbstractValidator<CreateScheduleRequestDto>
{
    public CreateScheduleRequestValidator()
    {
        RuleFor(x => x.DoctorId)
            .GreaterThan(0).WithMessage("DoctorId must be a valid ID.");

        RuleFor(x => x.BranchId)
            .GreaterThan(0).WithMessage("BranchId must be a valid ID.");

        RuleFor(x => x.DayOfWeek)
            .IsInEnum().WithMessage("DayOfWeek must be a valid day (0=Sunday .. 6=Saturday).");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.");

        RuleFor(x => x.SessionDurationMinutes)
            .GreaterThan(0).WithMessage("Session duration must be greater than zero.")
            .LessThanOrEqualTo(480).WithMessage("Session duration must not exceed 480 minutes.");

        RuleFor(x => x.BreakDurationMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Break duration cannot be negative.")
            .LessThanOrEqualTo(120).WithMessage("Break duration must not exceed 120 minutes.")
            .When(x => x.BreakDurationMinutes.HasValue);
    }
}

public class UpdateScheduleRequestValidator : AbstractValidator<UpdateScheduleRequestDto>
{
    public UpdateScheduleRequestValidator()
    {
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.");

        RuleFor(x => x.SessionDurationMinutes)
            .GreaterThan(0).WithMessage("Session duration must be greater than zero.")
            .LessThanOrEqualTo(480).WithMessage("Session duration must not exceed 480 minutes.");
    }
}

public class CreateScheduleChangeRequestValidator : AbstractValidator<CreateScheduleChangeRequestDto>
{
    public CreateScheduleChangeRequestValidator()
    {
        RuleFor(x => x.RequestingAdminId).GreaterThan(0).WithMessage("RequestingAdminId must be a valid ID.");
        RuleFor(x => x.OwnerId).GreaterThan(0).WithMessage("OwnerId must be a valid ID.");
        RuleFor(x => x.ScheduleId).GreaterThan(0).WithMessage("ScheduleId must be a valid ID.");

        RuleFor(x => x.ProposedDayOfWeek)
            .IsInEnum().WithMessage("Invalid ProposedDayOfWeek.")
            .When(x => x.ProposedDayOfWeek.HasValue);

        RuleFor(x => x.ProposedEndTime)
            .GreaterThan(x => x.ProposedStartTime!.Value)
            .WithMessage("ProposedEndTime must be after ProposedStartTime.")
            .When(x => x.ProposedStartTime.HasValue && x.ProposedEndTime.HasValue);

        RuleFor(x => x.ProposedSessionDurationMinutes)
            .GreaterThan(0).WithMessage("Session duration must be greater than zero.")
            .LessThanOrEqualTo(480).WithMessage("Session duration must not exceed 480 minutes.")
            .When(x => x.ProposedSessionDurationMinutes.HasValue);

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => x.Reason != null);
    }
}

public class SubmitScheduleChangeRequestValidator : AbstractValidator<SubmitScheduleChangeRequestDto>
{
    public SubmitScheduleChangeRequestValidator()
    {
        RuleFor(x => x.NewDayOfWeek)
            .IsInEnum().WithMessage("Invalid NewDayOfWeek.")
            .When(x => x.NewDayOfWeek.HasValue);

        RuleFor(x => x.NewEndTime)
            .GreaterThan(x => x.NewStartTime!.Value)
            .WithMessage("NewEndTime must be after NewStartTime.")
            .When(x => x.NewStartTime.HasValue && x.NewEndTime.HasValue);

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => x.Reason != null);
    }
}
