using Fcg.Contracts.Events.V1;
using MassTransit;

namespace Fcg.Notifications.WebApi.Messaging.UserCreated;

public sealed class UserCreatedConsumer(ILogger<UserCreatedConsumer> logger) : IConsumer<UserCreatedEvent>
{
    public Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var user = context.Message;

        logger.LogInformation(
            "E-mail de boas-vindas simulado para o usuário {UserId}. Evento {EventId}.",
            user.UserId,
            user.EventId);

        return Task.CompletedTask;
    }
}
