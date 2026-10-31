using Fcg.Contracts.Events.V1;
using Fcg.Notifications.Application.Notifications.WelcomeUser;

namespace Fcg.Notifications.UnitTests.Application.Notifications.WelcomeUser;

public class WelcomeUserCommandParserTests
{
    [Fact]
    public void Parse_DeveMapearEvento_QuandoUserCreatedEventInformado()
    {
        var integrationEvent = new UserCreatedEvent(Guid.CreateVersion7(), DateTime.UtcNow, Guid.CreateVersion7(), "Ada", "ada@fcg.dev");

        var command = new WelcomeUserCommandParser().Parse(integrationEvent);

        command.Should().Be(new WelcomeUserCommand(integrationEvent.EventId, integrationEvent.UserId, "Ada", "ada@fcg.dev"));
    }
}
