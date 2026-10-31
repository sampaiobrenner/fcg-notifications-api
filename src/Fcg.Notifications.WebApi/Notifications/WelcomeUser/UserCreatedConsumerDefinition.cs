using Fcg.Contracts.Messaging;
using MassTransit;

namespace Fcg.Notifications.WebApi.Notifications.WelcomeUser;

/// <summary>
/// Fila <c>notifications.user-created</c>: nome lido de <c>Messaging:Queues:NotificationsUserCreated</c>
/// (ConfigMap <c>Messaging__Queues__NotificationsUserCreated</c>), com default no contrato.
/// </summary>
public sealed class UserCreatedConsumerDefinition : ConsumerDefinition<UserCreatedConsumer>
{
    public UserCreatedConsumerDefinition(IConfiguration configuration)
        => EndpointName = configuration[$"Messaging:Queues:{nameof(QueueNames.NotificationsUserCreated)}"] ?? QueueNames.NotificationsUserCreated;
}
