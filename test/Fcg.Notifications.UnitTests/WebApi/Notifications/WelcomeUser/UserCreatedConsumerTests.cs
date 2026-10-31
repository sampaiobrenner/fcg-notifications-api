using Fcg.Contracts.Events.V1;
using Fcg.Contracts.Messaging;
using Fcg.Notifications.Application.Notifications.WelcomeUser;
using Fcg.Notifications.WebApi.Notifications.WelcomeUser;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Fcg.Notifications.UnitTests.WebApi.Notifications.WelcomeUser;

public class UserCreatedConsumerTests
{
    [Fact]
    public async Task Consume_DeveEnviarComando_QuandoEventoRecebido()
    {
        var integrationEvent = new UserCreatedEvent(Guid.CreateVersion7(), DateTime.UtcNow, Guid.CreateVersion7(), "Ada", "ada@fcg.dev");
        var sender = new Mock<ISender>();
        var context = new Mock<ConsumeContext<UserCreatedEvent>>();
        context.Setup(consumeContext => consumeContext.Message).Returns(integrationEvent);
        context.Setup(consumeContext => consumeContext.CancellationToken).Returns(CancellationToken.None);

        await new UserCreatedConsumer(sender.Object, new WelcomeUserCommandParser()).Consume(context.Object);

        sender.Verify(mediator => mediator.Send(
            new WelcomeUserCommand(integrationEvent.EventId, integrationEvent.UserId, "Ada", "ada@fcg.dev"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Definition_DeveUsarFilaDoContrato_QuandoConfiguracaoAusente()
    {
        var definition = new UserCreatedConsumerDefinition(new ConfigurationBuilder().Build());

        EndpointNameOf(definition).Should().Be(QueueNames.NotificationsUserCreated).And.Be("notifications.user-created");
    }

    [Fact]
    public void Definition_DeveUsarFilaDaConfiguracao_QuandoInformada()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Messaging:Queues:NotificationsUserCreated"] = "fila-customizada" })
            .Build();

        EndpointNameOf(new UserCreatedConsumerDefinition(configuration)).Should().Be("fila-customizada");
    }

    private static string EndpointNameOf(IConsumerDefinition definition)
        => definition.GetEndpointName(KebabCaseEndpointNameFormatter.Instance);
}
