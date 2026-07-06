using DCMS.Application.DTOs.ModificationRequests;
using FluentValidation;

namespace DCMS.Application.Validators;

public class CreateServiceModificationRequestValidator : AbstractValidator<CreateServiceModificationRequestDto>
{
    public CreateServiceModificationRequestValidator()
    {
        RuleFor(x => x.AdminId).GreaterThan(0).WithMessage("AdminId must be a valid ID.");
        RuleFor(x => x.OwnerId).GreaterThan(0).WithMessage("OwnerId must be a valid ID.");
        RuleFor(x => x.RequestType).IsInEnum().WithMessage("Invalid RequestType.");

        RuleFor(x => x.ProposedName)
            .MaximumLength(200).WithMessage("ProposedName must not exceed 200 characters.")
            .When(x => x.ProposedName != null);

        RuleFor(x => x.ProposedPrice)
            .GreaterThanOrEqualTo(0).WithMessage("ProposedPrice cannot be negative.")
            .When(x => x.ProposedPrice != null);

        RuleFor(x => x.ProposedDescription)
            .MaximumLength(1000).WithMessage("ProposedDescription must not exceed 1000 characters.")
            .When(x => x.ProposedDescription != null);
    }
}

public class CreateFAQModificationRequestValidator : AbstractValidator<CreateFAQModificationRequestDto>
{
    public CreateFAQModificationRequestValidator()
    {
        RuleFor(x => x.AdminId).GreaterThan(0).WithMessage("AdminId must be a valid ID.");
        RuleFor(x => x.OwnerId).GreaterThan(0).WithMessage("OwnerId must be a valid ID.");
        RuleFor(x => x.RequestType).IsInEnum().WithMessage("Invalid RequestType.");

        RuleFor(x => x.ProposedQuestion)
            .MaximumLength(500).WithMessage("ProposedQuestion must not exceed 500 characters.")
            .When(x => x.ProposedQuestion != null);

        RuleFor(x => x.ProposedAnswer)
            .MaximumLength(2000).WithMessage("ProposedAnswer must not exceed 2000 characters.")
            .When(x => x.ProposedAnswer != null);
    }
}

public class CreateOfferDiscountModificationRequestValidator : AbstractValidator<CreateOfferDiscountModificationRequestDto>
{
    public CreateOfferDiscountModificationRequestValidator()
    {
        RuleFor(x => x.AdminId).GreaterThan(0).WithMessage("AdminId must be a valid ID.");
        RuleFor(x => x.OwnerId).GreaterThan(0).WithMessage("OwnerId must be a valid ID.");
        RuleFor(x => x.RequestType).IsInEnum().WithMessage("Invalid RequestType.");

        RuleFor(x => x.ProposedTitle)
            .MaximumLength(200).WithMessage("ProposedTitle must not exceed 200 characters.")
            .When(x => x.ProposedTitle != null);

        RuleFor(x => x.ProposedDiscountPercentage)
            .GreaterThan(0).WithMessage("ProposedDiscountPercentage must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("ProposedDiscountPercentage cannot exceed 100.")
            .When(x => x.ProposedDiscountPercentage != null);

        RuleFor(x => x.ProposedEndDate)
            .GreaterThanOrEqualTo(x => x.ProposedStartDate)
            .WithMessage("ProposedEndDate must be on or after ProposedStartDate.")
            .When(x => x.ProposedStartDate != null && x.ProposedEndDate != null);
    }
}

public class CreateBranchModificationRequestValidator : AbstractValidator<CreateBranchModificationRequestDto>
{
    public CreateBranchModificationRequestValidator()
    {
        RuleFor(x => x.AdminId).GreaterThan(0).WithMessage("AdminId must be a valid ID.");
        RuleFor(x => x.OwnerId).GreaterThan(0).WithMessage("OwnerId must be a valid ID.");
        RuleFor(x => x.RequestType).IsInEnum().WithMessage("Invalid RequestType.");

        RuleFor(x => x.ProposedName)
            .MaximumLength(150).WithMessage("ProposedName must not exceed 150 characters.")
            .When(x => x.ProposedName != null);

        RuleFor(x => x.ProposedLocation)
            .MaximumLength(300).WithMessage("ProposedLocation must not exceed 300 characters.")
            .When(x => x.ProposedLocation != null);

        RuleFor(x => x.ProposedPhone)
            .MaximumLength(20).WithMessage("ProposedPhone must not exceed 20 characters.")
            .When(x => x.ProposedPhone != null);

        RuleFor(x => x.ProposedWorkingHours)
            .MaximumLength(200).WithMessage("ProposedWorkingHours must not exceed 200 characters.")
            .When(x => x.ProposedWorkingHours != null);
    }
}

public class ApproveRejectModificationRequestValidator : AbstractValidator<ApproveRejectModificationRequestDto>
{
    public ApproveRejectModificationRequestValidator()
    {
        RuleFor(x => x.RejectionReason)
            .MaximumLength(500).WithMessage("RejectionReason must not exceed 500 characters.")
            .When(x => !x.Approve && x.RejectionReason != null);
    }
}
