using FluentValidation;

namespace Fcg.Notifications.Application.Notifications.NotifyPayment;

internal sealed class NotifyPaymentCommandValidator : AbstractValidator<NotifyPaymentCommand>
{
    public NotifyPaymentCommandValidator()
    {
        RuleFor(command => command.EventId).NotEmpty();
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Price).GreaterThanOrEqualTo(0);
        RuleFor(command => command.Status).IsInEnum();
    }
}
