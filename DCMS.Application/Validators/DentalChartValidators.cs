using DCMS.Application.DTOs.DentalChart;
using FluentValidation;

namespace DCMS.Application.Validators;

public class UpdateDentalChartRequestValidator : AbstractValidator<UpdateDentalChartRequestDto>
{
    public UpdateDentalChartRequestValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.")
            .When(x => x.Notes != null);
    }
}

public class UpsertToothRecordRequestValidator : AbstractValidator<UpsertToothRecordRequestDto>
{
    public UpsertToothRecordRequestValidator()
    {
        RuleFor(x => x.ToothNumber)
            .InclusiveBetween(1, 32).WithMessage("ToothNumber must be between 1 and 32.");

        RuleFor(x => x.ToothStatus)
            .IsInEnum().WithMessage("Invalid Tooth Status.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => x.Notes != null);
    }
}
