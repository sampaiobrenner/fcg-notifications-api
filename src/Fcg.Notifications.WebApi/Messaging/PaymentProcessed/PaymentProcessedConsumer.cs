using Fcg.Contracts.Events.V1;
using MassTransit;

namespace Fcg.Notifications.WebApi.Messaging.PaymentProcessed;

public sealed class PaymentProcessedConsumer(ILogger<PaymentProcessedConsumer> logger)
    : IConsumer<PaymentProcessedEvent>
{
    public Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var payment = context.Message;

        if (payment.Status != PaymentStatus.Approved)
        {
            logger.LogInformation(
                "Notificação de compra não enviada: pagamento rejeitado para o pedido {OrderId}. Motivo: {Reason}. Evento {EventId}.",
                payment.OrderId,
                payment.Reason,
                payment.EventId);

            return Task.CompletedTask;
        }

        logger.LogInformation(
            "E-mail de confirmação de compra simulado para o usuário {UserId}: jogo {GameId}, pedido {OrderId}, valor {Price}. Evento {EventId}.",
            payment.UserId,
            payment.GameId,
            payment.OrderId,
            payment.Price,
            payment.EventId);

        return Task.CompletedTask;
    }
}
