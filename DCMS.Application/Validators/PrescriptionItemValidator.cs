using DCMS.Application.DTOs.Prescriptions;
using FluentValidation;

namespace DCMS.Application.Validators;

public class PrescriptionItemValidator : AbstractValidator<CreatePrescriptionItemRequestDto>
{
    public PrescriptionItemValidator()
    {
        // BR-40: MedicationName required, max 200
        RuleFor(x => x.MedicationName)
            .NotEmpty().WithMessage("MedicationName is required.")
            .MaximumLength(200).WithMessage("MedicationName must not exceed 200 characters.");

        // BR-40: Dosage required, max 200
        RuleFor(x => x.Dosage)
            .NotEmpty().WithMessage("Dosage is required.")
            .MaximumLength(200).WithMessage("Dosage must not exceed 200 characters.");

        RuleFor(x => x.Route)
            .IsInEnum().WithMessage("MedicationRoute is invalid.");
    }
}
