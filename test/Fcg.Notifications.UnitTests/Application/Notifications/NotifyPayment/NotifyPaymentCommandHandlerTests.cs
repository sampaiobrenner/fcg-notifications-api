using Fcg.Contracts.Events.V1;
using Fcg.Notifications.Application._Shared.Contexts;
using Fcg.Notifications.Application._Shared.Email;
using Fcg.Notifications.Application._Shared.Exceptions;
using Fcg.Notifications.Application.Notifications.NotifyPayment;
using Fcg.Notifications.Domain.EmailLogs;
using Fcg.Notifications.Domain.KnownUsers;
using Fcg.Notifications.UnitTests._Fakes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Fcg.Notifications.UnitTests.Application.Notifications.NotifyPayment;

public class NotifyPaymentCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 1, 12, 5, 0, TimeSpan.Zero);

    private readonly Mock<IKnownUserRepository> _knownUserRepository = new();
    private readonly Mock<IEmailLogRepository> _emailLogRepository = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<INotificationsDbContext> _context = new();
    private readonly RecordingLogger<NotifyPaymentCommandHandler> _logger = new();
    private readonly NotifyPaymentCommandHandler _handler;

    public NotifyPaymentCommandHandlerTests()
    {
        _handler = new NotifyPaymentCommandHandler(
            _knownUserRepository.Object,
            _emailLogRepository.Object,
            _emailSender.Object,
            _context.Object,
            new FakeTimeProvider(Now),
            _logger);
    }

    [Fact]
    public async Task Handle_DeveEnviarConfirmacaoDeCompra_QuandoPagamentoAprovado()
    {
        var command = Approved(price: 199.9m);
        GivenKnownUser(command.UserId, "Ada Lovelace", "ada@fcg.dev");
        EmailLog? logged = null;
        _emailLogRepository.Setup(repository => repository.Add(It.IsAny<EmailLog>())).Callback<EmailLog>(log => logged = log);

        await _handler.Handle(command, CancellationToken.None);

        _emailSender.Verify(sender => sender.SendAsync(
            new EmailMessage(
                "ada@fcg.dev",
                $"Compra confirmada - pedido {command.OrderId}",
                "Ola Ada Lovelace, o pagamento de R$ 199,90 foi aprovado e o jogo ja esta disponivel na sua biblioteca."),
            It.IsAny<CancellationToken>()), Times.Once);

        logged.Should().NotBeNull();
        logged!.PersistenceModel.Should().BeEquivalentTo(new
        {
            command.EventId,
            Kind = EmailKind.PurchaseConfirmed,
            ToEmail = "ada@fcg.dev",
            Subject = $"Compra confirmada - pedido {command.OrderId}",
            SentAt = Now.UtcDateTime
        });
        _context.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveApenasLogar_QuandoPagamentoRejeitado()
    {
        var command = Approved() with { Status = PaymentStatus.Rejected, Reason = "Valor acima do limite" };

        await _handler.Handle(command, CancellationToken.None);

        var entry = _logger.Entries.Should().ContainSingle().Subject;
        entry.Level.Should().Be(LogLevel.Information);
        entry.Message.Should().Be($"Pagamento {command.OrderId} rejeitado: Valor acima do limite");

        _knownUserRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailSender.Verify(sender => sender.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailLogRepository.Verify(repository => repository.Add(It.IsAny<EmailLog>()), Times.Never);
        _context.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveLancarUserNotKnownYetException_QuandoUsuarioAusenteNaProjecao()
    {
        var command = Approved();

        var act = () => _handler.Handle(command, CancellationToken.None);

        (await act.Should().ThrowAsync<UserNotKnownYetException>()).Which.UserId.Should().Be(command.UserId);
        _emailSender.Verify(sender => sender.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NaoDeveReenviar_QuandoEventoJaProcessado()
    {
        var command = Approved();
        GivenKnownUser(command.UserId, "Ada", "ada@fcg.dev");
        _emailLogRepository.Setup(repository => repository.ExistsByEventIdAsync(command.EventId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _handler.Handle(command, CancellationToken.None);

        _emailSender.Verify(sender => sender.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailLogRepository.Verify(repository => repository.Add(It.IsAny<EmailLog>()), Times.Never);
        _context.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static NotifyPaymentCommand Approved(decimal price = 59.9m)
        => new(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), price, PaymentStatus.Approved, null);

    private void GivenKnownUser(Guid userId, string name, string email)
        => _knownUserRepository
            .Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(KnownUser.Create(userId, name, email, Now.UtcDateTime));
}
