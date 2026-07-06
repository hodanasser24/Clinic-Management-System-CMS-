using DCMS.Application.DTOs.Prescriptions;
using FluentValidation;

namespace DCMS.Application.Validators;

public class CreatePrescriptionValidator : AbstractValidator<CreatePrescriptionRequestDto>
{
    public CreatePrescriptionValidator()
    {
        // BR-39: ReportId basic check — DB existence is validated in PrescriptionService (throws NotFoundException)
        RuleFor(x => x.ReportId)
            .GreaterThan(0).WithMessage("ReportId must be a valid ID.");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("Prescription must contain items.")
            .Must(items => items != null && items.Any())
            .WithMessage("Prescription must contain at least one item.");

        RuleForEach(x => x.Items)
            .SetValidator(new PrescriptionItemValidator());
    }
}
