using Fcg.Contracts.Events.V1;
using Fcg.Notifications.Application._Shared.Parsers;

namespace Fcg.Notifications.Application.Notifications.WelcomeUser;

public sealed class WelcomeUserCommandParser : Parser<UserCreatedEvent, WelcomeUserCommand>
{
    protected override WelcomeUserCommand DoParse(UserCreatedEvent source)
        => new(source.EventId, source.UserId, source.Name, source.Email);
}
