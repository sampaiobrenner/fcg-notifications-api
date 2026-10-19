using Fcg.Contracts.Messaging;
using Fcg.Notifications.WebApi._Shared.Messaging;
using MassTransit;

namespace Fcg.Notifications.WebApi.Messaging.UserCreated;

public sealed class UserCreatedConsumerDefinition : ConsumerDefinition<UserCreatedConsumer>
{
    public UserCreatedConsumerDefinition(IConfiguration configuration)
    {
        EndpointName = QueueNameConfiguration.GetQueueName(
            configuration,
            nameof(QueueNames.NotificationsUserCreated),
            QueueNames.NotificationsUserCreated);
    }
}
