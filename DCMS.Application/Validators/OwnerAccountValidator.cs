using DCMS.Application.DTOs.Owner;
using FluentValidation;

namespace DCMS.Application.Validators;

public class CreateDoctorAccountValidator : AbstractValidator<CreateDoctorAccountRequestDto>
{
    public CreateDoctorAccountValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).WithMessage("Password must be at least 8 characters.");
        RuleFor(x => x.Specialization).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Qualification).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ExperienceYears).GreaterThanOrEqualTo(0).LessThanOrEqualTo(60);
    }
}

public class CreateAdminAccountValidator : AbstractValidator<CreateAdminAccountRequestDto>
{
    public CreateAdminAccountValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).WithMessage("Password must be at least 8 characters.");
    }
}

public class DeactivateAccountRequestValidator : AbstractValidator<DeactivateAccountRequestDto>
{
    public DeactivateAccountRequestValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => x.Reason != null);
    }
}

public class ActivateOfferRequestValidator : AbstractValidator<ActivateOfferRequestDto>
{
    public ActivateOfferRequestValidator()
    {
        RuleFor(x => x.OfferId).GreaterThan(0).WithMessage("OfferId must be a valid ID.");
    }
}
