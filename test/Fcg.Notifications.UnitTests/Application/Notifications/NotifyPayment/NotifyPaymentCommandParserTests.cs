using Fcg.Contracts.Events.V1;
using Fcg.Notifications.Application.Notifications.NotifyPayment;

namespace Fcg.Notifications.UnitTests.Application.Notifications.NotifyPayment;

public class NotifyPaymentCommandParserTests
{
    [Fact]
    public void Parse_DeveMapearEvento_QuandoPaymentProcessedEventInformado()
    {
        var integrationEvent = new PaymentProcessedEvent(
            Guid.CreateVersion7(), DateTime.UtcNow, Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            399.9m, PaymentStatus.Rejected, "Valor acima do limite");

        var command = new NotifyPaymentCommandParser().Parse(integrationEvent);

        command.Should().Be(new NotifyPaymentCommand(
            integrationEvent.EventId, integrationEvent.OrderId, integrationEvent.UserId, 399.9m, PaymentStatus.Rejected, "Valor acima do limite"));
    }
}
