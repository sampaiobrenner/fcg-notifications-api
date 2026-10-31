using Fcg.Contracts.Events.V1;
using Fcg.Notifications.Application._Shared.Parsers;

namespace Fcg.Notifications.Application.Notifications.NotifyPayment;

public sealed class NotifyPaymentCommandParser : Parser<PaymentProcessedEvent, NotifyPaymentCommand>
{
    protected override NotifyPaymentCommand DoParse(PaymentProcessedEvent source)
        => new(source.EventId, source.OrderId, source.UserId, source.Price, source.Status, source.Reason);
}
