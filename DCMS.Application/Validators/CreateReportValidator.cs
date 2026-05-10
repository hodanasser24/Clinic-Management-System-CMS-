using DCMS.Application.DTOs.Reports;
using FluentValidation;

namespace DCMS.Application.Validators;

public class CreateReportValidator : AbstractValidator<CreateReportRequestDto>
{
    public CreateReportValidator()
    {
        RuleFor(x => x.AppointmentId).GreaterThan(0).WithMessage("AppointmentId must be a valid ID.");
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("PatientId must be a valid ID.");
        RuleFor(x => x.DoctorId).GreaterThan(0).WithMessage("DoctorId must be a valid ID.");
        // BR-41: Diagnosis max 2000
        RuleFor(x => x.Diagnosis)
            .NotEmpty().WithMessage("Diagnosis is required.")
            .MaximumLength(2000).WithMessage("Diagnosis must not exceed 2000 characters.");
        // BR-57: InternalNotes max 3000
        RuleFor(x => x.InternalNotes)
            .MaximumLength(3000).WithMessage("InternalNotes must not exceed 3000 characters.")
            .When(x => x.InternalNotes != null);
        RuleFor(x => x.CaseStatus).IsInEnum().WithMessage("CaseStatus is invalid.");
    }
}
