using DCMS.Application.DTOs.Owner;
using DCMS.Application.DTOs.Profile;
using FluentValidation;

namespace DCMS.Application.Validators;

public class UpdatePatientProfileRequestValidator : AbstractValidator<UpdatePatientProfileRequestDto>
{
    public UpdatePatientProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .When(x => x.Phone != null);

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth cannot be in the future.");

        RuleFor(x => x.MedicalHistory)
            .MaximumLength(2000).WithMessage("Medical history must not exceed 2000 characters.")
            .When(x => x.MedicalHistory != null);
    }
}

public class UpdateDoctorSelfProfileRequestValidator : AbstractValidator<UpdateDoctorSelfProfileRequestDto>
{
    public UpdateDoctorSelfProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .When(x => x.Phone != null);

        RuleFor(x => x.Specialization)
            .NotEmpty().WithMessage("Specialization is required.")
            .MaximumLength(200).WithMessage("Specialization must not exceed 200 characters.");

        RuleFor(x => x.Qualification)
            .NotEmpty().WithMessage("Qualification is required.")
            .MaximumLength(200).WithMessage("Qualification must not exceed 200 characters.");

        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Bio must not exceed 1000 characters.")
            .When(x => x.Bio != null);

        RuleFor(x => x.ExperienceYears)
            .GreaterThanOrEqualTo(0).WithMessage("Experience years cannot be negative.")
            .LessThanOrEqualTo(70).WithMessage("Experience years must not exceed 70.");
    }
}

public class UpdatePhotoRequestValidator : AbstractValidator<UpdatePhotoRequestDto>
{
    public UpdatePhotoRequestValidator()
    {
        RuleFor(x => x.PhotoUrl)
            .NotEmpty().WithMessage("Photo URL is required.")
            .MaximumLength(500).WithMessage("Photo URL must not exceed 500 characters.");
    }
}

public class UpdateDoctorProfileRequestValidator : AbstractValidator<UpdateDoctorProfileRequestDto>
{
    public UpdateDoctorProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .When(x => x.Phone != null);

        RuleFor(x => x.Specialization)
            .NotEmpty().WithMessage("Specialization is required.")
            .MaximumLength(200).WithMessage("Specialization must not exceed 200 characters.");

        RuleFor(x => x.Qualification)
            .NotEmpty().WithMessage("Qualification is required.")
            .MaximumLength(200).WithMessage("Qualification must not exceed 200 characters.");

        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Bio must not exceed 1000 characters.")
            .When(x => x.Bio != null);

        RuleFor(x => x.ExperienceYears)
            .GreaterThanOrEqualTo(0).WithMessage("Experience years cannot be negative.")
            .LessThanOrEqualTo(70).WithMessage("Experience years must not exceed 70.");
    }
}

public class UpdateDoctorPhotoRequestValidator : AbstractValidator<UpdateDoctorPhotoRequestDto>
{
    public UpdateDoctorPhotoRequestValidator()
    {
        RuleFor(x => x.PhotoUrl)
            .NotEmpty().WithMessage("Photo URL is required.")
            .MaximumLength(500).WithMessage("Photo URL must not exceed 500 characters.");
    }
}
