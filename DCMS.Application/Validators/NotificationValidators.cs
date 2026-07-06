using DCMS.Application.DTOs.Notifications;
using FluentValidation;

namespace DCMS.Application.Validators;

public class MarkNotificationReadRequestValidator : AbstractValidator<MarkNotificationReadRequestDto>
{
    public MarkNotificationReadRequestValidator()
    {
        RuleFor(x => x.NotificationId).GreaterThan(0).WithMessage("NotificationId must be a valid ID.");
    }
}
