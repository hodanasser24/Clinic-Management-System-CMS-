using DCMS.Application.DTOs.Appointments;
using FluentValidation;

namespace DCMS.Application.Validators;

public class AppointmentRequestValidator : AbstractValidator<AppointmentRequestDto>
{
    public AppointmentRequestValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("PatientId must be a valid ID.");
        RuleFor(x => x.DoctorId).GreaterThan(0).WithMessage("DoctorId must be a valid ID.");
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage("BranchId must be a valid ID.");
        RuleFor(x => x.ServiceId).GreaterThan(0).WithMessage("ServiceId must be a valid ID.");
        RuleFor(x => x.Date)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Appointment date cannot be in the past.");
        // BR-52: Notes max 1000 chars
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => x.Notes != null);
    }
}

public class RescheduleAppointmentValidator : AbstractValidator<RescheduleAppointmentRequestDto>
{
    public RescheduleAppointmentValidator()
    {
        RuleFor(x => x.NewDate)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("New date cannot be in the past.");
    }
}

public class CancelAppointmentRequestValidator : AbstractValidator<CancelAppointmentRequestDto>
{
    public CancelAppointmentRequestValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => x.Reason != null);
    }
}

public class ConfirmAppointmentRequestValidator : AbstractValidator<ConfirmAppointmentRequestDto>
{
    public ConfirmAppointmentRequestValidator()
    {
        RuleFor(x => x.AdminId).GreaterThan(0).WithMessage("AdminId must be a valid ID.");
    }
}

public class RejectAppointmentRequestValidator : AbstractValidator<RejectAppointmentRequestDto>
{
    public RejectAppointmentRequestValidator()
    {
        RuleFor(x => x.AdminId).GreaterThan(0).WithMessage("AdminId must be a valid ID.");
    }
}

public class MarkUrgentRequestValidator : AbstractValidator<MarkUrgentRequestDto>
{
    public MarkUrgentRequestValidator()
    {
        RuleFor(x => x.DoctorId).GreaterThan(0).WithMessage("DoctorId must be a valid ID.");
    }
}

public class MarkAttendanceRequestValidator : AbstractValidator<MarkAttendanceRequestDto>
{
    public MarkAttendanceRequestValidator()
    {
        RuleFor(x => x.AttendanceStatus)
            .IsInEnum().WithMessage("Invalid Attendance Status.");
    }
}
