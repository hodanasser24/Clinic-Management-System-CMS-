using DCMS.Application.DTOs;
using DCMS.Domain.Enums;
using FluentValidation;

namespace DCMS.Application.Validators;

public class CreateContactMessageValidator : AbstractValidator<CreateContactMessageRequest>
{
    public CreateContactMessageValidator()
    {
        RuleFor(x => x.SenderName)
            .NotEmpty().WithMessage("Sender name is required.")
            .MaximumLength(100);

        RuleFor(x => x.SenderEmail)
            .NotEmpty().WithMessage("Sender email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(150);

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(200);

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Message body is required.")
            .MaximumLength(4000);

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid message type.");
    }
}

public class ReplyContactMessageValidator : AbstractValidator<ReplyContactMessageRequest>
{
    public ReplyContactMessageValidator()
    {
        RuleFor(x => x.ReplyBody)
            .NotEmpty().WithMessage("Reply body is required.")
            .MaximumLength(4000);
    }
}

public class UpdateContactMessageStatusValidator : AbstractValidator<UpdateContactMessageStatusRequest>
{
    public UpdateContactMessageStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value.");
    }
}

public class ContactMessageFilterValidator : AbstractValidator<ContactMessageFilterRequest>
{
    public ContactMessageFilterValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        When(x => x.Type.HasValue, () =>
            RuleFor(x => x.Type!.Value).IsInEnum().WithMessage("Invalid message type."));

        When(x => x.Status.HasValue, () =>
            RuleFor(x => x.Status!.Value).IsInEnum().WithMessage("Invalid status."));

        When(x => x.From.HasValue && x.To.HasValue, () =>
            RuleFor(x => x.To!.Value)
                .GreaterThanOrEqualTo(x => x.From!.Value)
                .WithMessage("'To' date must be on or after 'From' date."));
    }
}
