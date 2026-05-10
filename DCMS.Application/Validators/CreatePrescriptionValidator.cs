using DCMS.Application.DTOs.Prescriptions;
using DCMS.Domain.Interfaces;
using FluentValidation;

namespace DCMS.Application.Validators;

public class CreatePrescriptionValidator : AbstractValidator<CreatePrescriptionRequestDto>
{
    public CreatePrescriptionValidator(IUnitOfWork uow)
    {
        // BR-39: Prescription requires existing Report (MustAsync check)
        RuleFor(x => x.ReportId)
            .GreaterThan(0).WithMessage("ReportId must be a valid ID.")
            .MustAsync(async (reportId, ct) =>
            {
                var report = await uow.Reports.GetByIdAsync(reportId, ct);
                return report != null;
            }).WithMessage("The specified Report does not exist.");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("Prescription must contain items.")
            .Must(items => items != null && items.Any())
            .WithMessage("Prescription must contain at least one item.");

        RuleForEach(x => x.Items)
            .SetValidator(new PrescriptionItemValidator());
    }
}
