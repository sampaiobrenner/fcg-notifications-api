using Fcg.Notifications.Application._Shared.Contexts;
using Fcg.Notifications.Application._Shared.Email;
using Fcg.Notifications.Application.Notifications.WelcomeUser;
using Fcg.Notifications.Domain.EmailLogs;
using Fcg.Notifications.Domain.KnownUsers;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Fcg.Notifications.UnitTests.Application.Notifications.WelcomeUser;

public class WelcomeUserCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IKnownUserRepository> _knownUserRepository = new();
    private readonly Mock<IEmailLogRepository> _emailLogRepository = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<INotificationsDbContext> _context = new();
    private readonly WelcomeUserCommandHandler _handler;

    public WelcomeUserCommandHandlerTests()
    {
        _handler = new WelcomeUserCommandHandler(
            _knownUserRepository.Object,
            _emailLogRepository.Object,
            _emailSender.Object,
            _context.Object,
            new FakeTimeProvider(Now));
    }

    [Fact]
    public async Task Handle_DeveCriarProjecaoEEnviarBoasVindas_QuandoUsuarioNovo()
    {
        var command = new WelcomeUserCommand(Guid.CreateVersion7(), Guid.CreateVersion7(), "Ada Lovelace", "ada@fcg.dev");
        KnownUser? added = null;
        EmailLog? logged = null;
        _knownUserRepository.Setup(repository => repository.Add(It.IsAny<KnownUser>())).Callback<KnownUser>(user => added = user);
        _emailLogRepository.Setup(repository => repository.Add(It.IsAny<EmailLog>())).Callback<EmailLog>(log => logged = log);

        await _handler.Handle(command, CancellationToken.None);

        added.Should().NotBeNull();
        added!.Id().Should().Be(command.UserId);
        added.Name.Should().Be("Ada Lovelace");
        added.Email.Should().Be("ada@fcg.dev");
        added.PersistenceModel.UpdatedAt.Should().Be(Now.UtcDateTime);

        _emailSender.Verify(sender => sender.SendAsync(
            new EmailMessage(
                "ada@fcg.dev",
                "Bem-vindo(a) a FIAP Cloud Games!",
                "Ola Ada Lovelace, sua conta foi criada com sucesso. Aproveite o catalogo de jogos!"),
            It.IsAny<CancellationToken>()), Times.Once);

        logged.Should().NotBeNull();
        logged!.PersistenceModel.EventId.Should().Be(command.EventId);
        logged.PersistenceModel.Kind.Should().Be(EmailKind.Welcome);
        logged.PersistenceModel.ToEmail.Should().Be("ada@fcg.dev");
        logged.PersistenceModel.SentAt.Should().Be(Now.UtcDateTime);

        _context.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveAtualizarProjecao_QuandoUsuarioJaConhecido()
    {
        var userId = Guid.CreateVersion7();
        var existing = KnownUser.Create(userId, "Ada", "ada@fcg.dev", Now.UtcDateTime.AddDays(-1));
        _knownUserRepository.Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        await _handler.Handle(new WelcomeUserCommand(Guid.CreateVersion7(), userId, "Ada King", "ada.king@fcg.dev"), CancellationToken.None);

        existing.Name.Should().Be("Ada King");
        existing.Email.Should().Be("ada.king@fcg.dev");
        existing.PersistenceModel.UpdatedAt.Should().Be(Now.UtcDateTime);
        _knownUserRepository.Verify(repository => repository.Add(It.IsAny<KnownUser>()), Times.Never);
        _context.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NaoDeveReenviarBoasVindas_QuandoEventoJaProcessado()
    {
        var command = new WelcomeUserCommand(Guid.CreateVersion7(), Guid.CreateVersion7(), "Ada", "ada@fcg.dev");
        _emailLogRepository.Setup(repository => repository.ExistsByEventIdAsync(command.EventId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _handler.Handle(command, CancellationToken.None);

        _emailSender.Verify(sender => sender.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailLogRepository.Verify(repository => repository.Add(It.IsAny<EmailLog>()), Times.Never);
        _context.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
