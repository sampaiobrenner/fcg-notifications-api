using Fcg.Contracts.Events.V1;
using Fcg.Notifications.Application._Shared.Parsers;
using Fcg.Notifications.Application.Notifications.WelcomeUser;
using MassTransit;
using MediatR;

namespace Fcg.Notifications.WebApi.Notifications.WelcomeUser;

public sealed class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly ISender _sender;
    private readonly IParser<UserCreatedEvent, WelcomeUserCommand> _parser;

    public UserCreatedConsumer(ISender sender, IParser<UserCreatedEvent, WelcomeUserCommand> parser)
    {
        _sender = sender;
        _parser = parser;
    }

    public Task Consume(ConsumeContext<UserCreatedEvent> context)
        => _sender.Send(_parser.Parse(context.Message)!, context.CancellationToken);
}
