using System.Text.Json;
using Fcg.Contracts.Events.V1;
using Fcg.Notifications.WebApi._Shared.Messaging;
using MassTransit.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Notifications.UnitTests.WebApi._Shared.Messaging;

public class ContractJsonConventionsTests
{
    private const string PaymentProcessedSample = """
        {
          "eventId": "0199a1b2-0000-7000-8000-000000000003",
          "occurredAt": "2026-10-01T12:05:02Z",
          "orderId": "0199a1b2-0000-7000-8000-0000000000c1",
          "userId": "0199a1b2-0000-7000-8000-0000000000a1",
          "gameId": "0199a1b2-0000-7000-8000-0000000000b1",
          "price": 199.90,
          "status": "Approved",
          "reason": null
        }
        """;

    [Fact]
    public void Serializer_DeveLerPaymentProcessedDoContrato_QuandoStatusEmTextoEPrecoNumerico()
    {
        new ServiceCollection().AddMessaging(new ConfigurationBuilder().Build());

        var evt = JsonSerializer.Deserialize<PaymentProcessedEvent>(PaymentProcessedSample, SystemTextJsonMessageSerializer.Options);

        evt!.Status.Should().Be(PaymentStatus.Approved);
        evt.Price.Should().Be(199.90m);
    }
}
