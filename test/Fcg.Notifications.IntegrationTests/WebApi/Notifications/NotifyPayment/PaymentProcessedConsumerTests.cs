using Fcg.Contracts.Events.V1;
using Fcg.Notifications.Application._Shared.Email;
using Fcg.Notifications.Domain.EmailLogs;
using Fcg.Notifications.Domain.KnownUsers;
using Fcg.Notifications.IntegrationTests._Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Notifications.IntegrationTests.WebApi.Notifications.NotifyPayment;

[Collection(NotificationsApiCollection.Name)]
public class PaymentProcessedConsumerTests
{
    private readonly NotificationsApiFixture _fixture;

    public PaymentProcessedConsumerTests(NotificationsApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Consume_DeveEnviarUmEmailDeConfirmacao_QuandoPagamentoAprovado()
    {
        var user = await GivenKnownUserAsync("Ada Lovelace");
        var payment = NewPayment(user.Id, PaymentStatus.Approved, 199.90m);

        await _fixture.WaitUntilConsumedAsync(await _fixture.PublishAsync(payment));

        _fixture.EmailSender.SentTo(user.Email).Should().ContainSingle().Which.Should().Be(new EmailMessage(
            user.Email,
            $"Compra confirmada - pedido {payment.OrderId}",
            "Ola Ada Lovelace, o pagamento de R$ 199,90 foi aprovado e o jogo ja esta disponivel na sua biblioteca."));

        (await EmailLogsAsync(payment.EventId)).Should().ContainSingle().Which.Kind.Should().Be(EmailKind.PurchaseConfirmed);
    }

    [Fact]
    public async Task Consume_NaoDeveEnviarEmail_QuandoPagamentoRejeitado()
    {
        var user = await GivenKnownUserAsync("Grace Hopper");
        var payment = NewPayment(user.Id, PaymentStatus.Rejected, 399.90m) with { Reason = "Valor acima do limite de aprovacao" };

        await _fixture.WaitUntilConsumedAsync(await _fixture.PublishAsync(payment));

        _fixture.EmailSender.SentTo(user.Email).Should().BeEmpty();
        (await EmailLogsAsync(payment.EventId)).Should().BeEmpty();
    }

    [Fact]
    public async Task Consume_DeveRetentarEEnviar_QuandoUsuarioChegaDepoisDoPagamento()
    {
        var userId = Guid.CreateVersion7();
        var email = $"{userId:N}@fcg.dev";
        var payment = NewPayment(userId, PaymentStatus.Approved, 59.90m);

        var paymentMessageId = await _fixture.PublishAsync(payment);

        // Primeira tentativa falha com UserNotKnownYetException (usuario fora da projecao) e entra no retry 1s/5s/15s/30s.
        await Task.Delay(TimeSpan.FromMilliseconds(1500));
        (await EmailLogsAsync(payment.EventId)).Should().BeEmpty();
        _fixture.EmailSender.SentTo(email).Should().BeEmpty();

        await _fixture.WaitUntilConsumedAsync(await _fixture.PublishAsync(
            new UserCreatedEvent(Guid.CreateVersion7(), DateTime.UtcNow, userId, "Margaret Hamilton", email)));

        await _fixture.WaitUntilConsumedAsync(paymentMessageId, TimeSpan.FromSeconds(45));

        _fixture.EmailSender.SentTo(email).Should().ContainSingle(message => message.Subject == $"Compra confirmada - pedido {payment.OrderId}");
        (await EmailLogsAsync(payment.EventId)).Should().ContainSingle();
    }

    [Fact]
    public async Task Consume_DeveEnviarApenasUmEmail_QuandoEventoDuplicado()
    {
        var user = await GivenKnownUserAsync("Katherine Johnson");
        var payment = NewPayment(user.Id, PaymentStatus.Approved, 99.90m);

        await _fixture.WaitUntilConsumedAsync(await _fixture.PublishAsync(payment));
        await _fixture.WaitUntilConsumedAsync(await _fixture.PublishAsync(payment));

        _fixture.EmailSender.SentTo(user.Email).Should().ContainSingle();
        (await EmailLogsAsync(payment.EventId)).Should().ContainSingle();
    }

    [Fact]
    public async Task Consumer_DeveEscutarFilaDoContrato_QuandoMensagemEnviadaParaNotificationsPaymentProcessed()
    {
        var user = await GivenKnownUserAsync("Hedy Lamarr");
        var payment = NewPayment(user.Id, PaymentStatus.Approved, 10m);

        await _fixture.WaitUntilConsumedAsync(await _fixture.SendToQueueAsync("notifications.payment-processed", payment));

        _fixture.EmailSender.SentTo(user.Email).Should().ContainSingle();
    }

    private async Task<KnownUserModel> GivenKnownUserAsync(string name)
    {
        var userId = Guid.CreateVersion7();
        var user = KnownUser.Create(userId, name, $"{userId:N}@fcg.dev", DateTime.UtcNow).PersistenceModel;

        await _fixture.QueryAsync(async context =>
        {
            context.Add(user);
            return await context.SaveChangesAsync();
        });

        return user;
    }

    private static PaymentProcessedEvent NewPayment(Guid userId, PaymentStatus status, decimal price)
        => new(Guid.CreateVersion7(), DateTime.UtcNow, Guid.CreateVersion7(), userId, Guid.CreateVersion7(), price, status, null);

    private Task<List<EmailLogModel>> EmailLogsAsync(Guid eventId)
        => _fixture.QueryAsync(context => context.Set<EmailLogModel>().AsNoTracking().Where(log => log.EventId == eventId).ToListAsync());
}
