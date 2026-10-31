using Fcg.Contracts.Events.V1;
using Fcg.Notifications.Application._Shared.Parsers;
using Fcg.Notifications.Application.Notifications.NotifyPayment;
using MassTransit;
using MediatR;

namespace Fcg.Notifications.WebApi.Notifications.NotifyPayment;

public sealed class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly ISender _sender;
    private readonly IParser<PaymentProcessedEvent, NotifyPaymentCommand> _parser;

    public PaymentProcessedConsumer(ISender sender, IParser<PaymentProcessedEvent, NotifyPaymentCommand> parser)
    {
        _sender = sender;
        _parser = parser;
    }

    public Task Consume(ConsumeContext<PaymentProcessedEvent> context)
        => _sender.Send(_parser.Parse(context.Message)!, context.CancellationToken);
}
