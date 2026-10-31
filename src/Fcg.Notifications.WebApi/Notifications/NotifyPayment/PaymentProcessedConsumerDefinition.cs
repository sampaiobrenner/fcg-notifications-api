using Fcg.Contracts.Messaging;
using MassTransit;

namespace Fcg.Notifications.WebApi.Notifications.NotifyPayment;

/// <summary>
/// Fila <c>notifications.payment-processed</c>: nome lido de <c>Messaging:Queues:NotificationsPaymentProcessed</c>
/// (ConfigMap <c>Messaging__Queues__NotificationsPaymentProcessed</c>), com default no contrato.
/// </summary>
public sealed class PaymentProcessedConsumerDefinition : ConsumerDefinition<PaymentProcessedConsumer>
{
    public PaymentProcessedConsumerDefinition(IConfiguration configuration)
        => EndpointName = configuration[$"Messaging:Queues:{nameof(QueueNames.NotificationsPaymentProcessed)}"] ?? QueueNames.NotificationsPaymentProcessed;
}
