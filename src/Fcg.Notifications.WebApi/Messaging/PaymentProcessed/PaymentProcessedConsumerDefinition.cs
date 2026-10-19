using Fcg.Contracts.Messaging;
using MassTransit;

namespace Fcg.Notifications.WebApi.Messaging.PaymentProcessed;

public sealed class PaymentProcessedConsumerDefinition : ConsumerDefinition<PaymentProcessedConsumer>
{
    public PaymentProcessedConsumerDefinition(IConfiguration configuration)
    {
        EndpointName = configuration[$"Messaging:Queues:{nameof(QueueNames.NotificationsPaymentProcessed)}"]
            ?? QueueNames.NotificationsPaymentProcessed;
    }
}
