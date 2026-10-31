using Fcg.Notifications.Domain.KnownUsers;
using FluentValidation;

namespace Fcg.Notifications.Application.Notifications.WelcomeUser;

internal sealed class WelcomeUserCommandValidator : AbstractValidator<WelcomeUserCommand>
{
    public WelcomeUserCommandValidator()
    {
        RuleFor(command => command.EventId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(KnownUserModel.NameMaxLength);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(KnownUserModel.EmailMaxLength);
    }
}
