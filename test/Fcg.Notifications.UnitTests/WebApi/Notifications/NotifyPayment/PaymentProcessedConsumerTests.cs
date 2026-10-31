using Fcg.Contracts.Events.V1;
using Fcg.Contracts.Messaging;
using Fcg.Notifications.Application.Notifications.NotifyPayment;
using Fcg.Notifications.WebApi.Notifications.NotifyPayment;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Fcg.Notifications.UnitTests.WebApi.Notifications.NotifyPayment;

public class PaymentProcessedConsumerTests
{
    [Fact]
    public async Task Consume_DeveEnviarComando_QuandoEventoRecebido()
    {
        var integrationEvent = new PaymentProcessedEvent(
            Guid.CreateVersion7(), DateTime.UtcNow, Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            199.9m, PaymentStatus.Approved, null);
        var sender = new Mock<ISender>();
        var context = new Mock<ConsumeContext<PaymentProcessedEvent>>();
        context.Setup(consumeContext => consumeContext.Message).Returns(integrationEvent);
        context.Setup(consumeContext => consumeContext.CancellationToken).Returns(CancellationToken.None);

        await new PaymentProcessedConsumer(sender.Object, new NotifyPaymentCommandParser()).Consume(context.Object);

        sender.Verify(mediator => mediator.Send(
            new NotifyPaymentCommand(integrationEvent.EventId, integrationEvent.OrderId, integrationEvent.UserId, 199.9m, PaymentStatus.Approved, null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Definition_DeveUsarFilaDoContrato_QuandoConfiguracaoAusente()
    {
        var definition = new PaymentProcessedConsumerDefinition(new ConfigurationBuilder().Build());

        EndpointNameOf(definition).Should().Be(QueueNames.NotificationsPaymentProcessed).And.Be("notifications.payment-processed");
    }

    [Fact]
    public void Definition_DeveUsarFilaDaConfiguracao_QuandoInformada()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Messaging:Queues:NotificationsPaymentProcessed"] = "fila-customizada" })
            .Build();

        EndpointNameOf(new PaymentProcessedConsumerDefinition(configuration)).Should().Be("fila-customizada");
    }

    private static string EndpointNameOf(IConsumerDefinition definition)
        => definition.GetEndpointName(KebabCaseEndpointNameFormatter.Instance);
}
