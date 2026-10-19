using Fcg.Contracts.Messaging;
using Fcg.Notifications.WebApi._Shared.Messaging;
using MassTransit;

namespace Fcg.Notifications.WebApi.Messaging.PaymentProcessed;

public sealed class PaymentProcessedConsumerDefinition : ConsumerDefinition<PaymentProcessedConsumer>
{
    public PaymentProcessedConsumerDefinition(IConfiguration configuration)
    {
        EndpointName = QueueNameConfiguration.GetQueueName(
            configuration,
            nameof(QueueNames.NotificationsPaymentProcessed),
            QueueNames.NotificationsPaymentProcessed);
    }
}
