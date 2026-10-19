using Fcg.Contracts.Messaging;
using MassTransit;

namespace Fcg.Notifications.WebApi.Messaging.UserCreated;

public sealed class UserCreatedConsumerDefinition : ConsumerDefinition<UserCreatedConsumer>
{
    public UserCreatedConsumerDefinition(IConfiguration configuration)
    {
        EndpointName = configuration[$"Messaging:Queues:{nameof(QueueNames.NotificationsUserCreated)}"]
            ?? QueueNames.NotificationsUserCreated;
    }
}
