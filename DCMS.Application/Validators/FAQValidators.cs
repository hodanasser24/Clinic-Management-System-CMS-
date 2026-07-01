using DCMS.Application.DTOs.FAQs;
using FluentValidation;

namespace DCMS.Application.Validators;

public class CreateFAQRequestValidator : AbstractValidator<CreateFAQRequestDto>
{
    public CreateFAQRequestValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Question is required.")
            .MaximumLength(500).WithMessage("Question must not exceed 500 characters.");

        RuleFor(x => x.Answer)
            .NotEmpty().WithMessage("Answer is required.")
            .MaximumLength(2000).WithMessage("Answer must not exceed 2000 characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order cannot be negative.");
    }
}

public class UpdateFAQRequestValidator : AbstractValidator<UpdateFAQRequestDto>
{
    public UpdateFAQRequestValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Question is required.")
            .MaximumLength(500).WithMessage("Question must not exceed 500 characters.");

        RuleFor(x => x.Answer)
            .NotEmpty().WithMessage("Answer is required.")
            .MaximumLength(2000).WithMessage("Answer must not exceed 2000 characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order cannot be negative.");
    }
}
