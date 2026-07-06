using DCMS.Application.DTOs.Offers;
using FluentValidation;

namespace DCMS.Application.Validators;

public class CreateOfferDiscountRequestValidator : AbstractValidator<CreateOfferDiscountRequestDto>
{
    public CreateOfferDiscountRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.DiscountPercentage)
            .GreaterThan(0).WithMessage("Discount percentage must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Discount percentage cannot exceed 100.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after the start date.");

        RuleFor(x => x.BranchId)
            .GreaterThan(0).WithMessage("BranchId must be a valid ID.");
    }
}

public class UpdateOfferDiscountRequestValidator : AbstractValidator<UpdateOfferDiscountRequestDto>
{
    public UpdateOfferDiscountRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.DiscountPercentage)
            .GreaterThan(0).WithMessage("Discount percentage must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Discount percentage cannot exceed 100.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after the start date.");
    }
}
