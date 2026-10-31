using Fcg.Contracts.Events.V1;
using Fcg.Contracts.Messaging;
using Fcg.Notifications.Application._Shared.Email;
using Fcg.Notifications.Domain.EmailLogs;
using Fcg.Notifications.Domain.KnownUsers;
using Fcg.Notifications.IntegrationTests._Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Notifications.IntegrationTests.WebApi.Notifications.WelcomeUser;

[Collection(NotificationsApiCollection.Name)]
public class UserCreatedConsumerTests
{
    private const string Subject = "Bem-vindo(a) a FIAP Cloud Games!";

    private readonly NotificationsApiFixture _fixture;

    public UserCreatedConsumerTests(NotificationsApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Consume_DeveGravarProjecaoEEnviarUmEmail_QuandoUsuarioNovo()
    {
        var integrationEvent = NewEvent(Guid.CreateVersion7(), "Ada Lovelace");

        var messageId = await _fixture.PublishAsync(integrationEvent);
        await _fixture.WaitUntilConsumedAsync(messageId);

        var knownUser = await FindKnownUserAsync(integrationEvent.UserId);
        knownUser.Should().NotBeNull();
        knownUser!.Name.Should().Be("Ada Lovelace");
        knownUser.Email.Should().Be(integrationEvent.Email);

        _fixture.EmailSender.SentTo(integrationEvent.Email).Should().ContainSingle().Which.Should().Be(new EmailMessage(
            integrationEvent.Email,
            Subject,
            "Ola Ada Lovelace, sua conta foi criada com sucesso. Aproveite o catalogo de jogos!"));

        var logs = await EmailLogsAsync(integrationEvent.EventId);
        logs.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            integrationEvent.EventId,
            Kind = EmailKind.Welcome,
            ToEmail = integrationEvent.Email,
            Subject
        });
    }

    [Fact]
    public async Task Consume_DeveEnviarApenasUmEmail_QuandoEventoDuplicado()
    {
        var integrationEvent = NewEvent(Guid.CreateVersion7(), "Grace Hopper");

        await _fixture.WaitUntilConsumedAsync(await _fixture.PublishAsync(integrationEvent));
        await _fixture.WaitUntilConsumedAsync(await _fixture.PublishAsync(integrationEvent));

        _fixture.EmailSender.SentTo(integrationEvent.Email).Should().ContainSingle();
        (await EmailLogsAsync(integrationEvent.EventId)).Should().ContainSingle();
    }

    [Fact]
    public async Task Consume_DeveAtualizarProjecao_QuandoMesmoUsuarioComNomeNovo()
    {
        var userId = Guid.CreateVersion7();
        var first = NewEvent(userId, "Ada");
        var second = first with { EventId = Guid.CreateVersion7(), Name = "Ada King", OccurredAt = DateTime.UtcNow };

        await _fixture.WaitUntilConsumedAsync(await _fixture.PublishAsync(first));
        var createdAt = (await FindKnownUserAsync(userId))!.UpdatedAt;

        await _fixture.WaitUntilConsumedAsync(await _fixture.PublishAsync(second));

        var knownUser = await FindKnownUserAsync(userId);
        knownUser!.Name.Should().Be("Ada King");
        knownUser.UpdatedAt.Should().BeOnOrAfter(createdAt);
        (await _fixture.QueryAsync(context => context.Set<KnownUserModel>().CountAsync(user => user.Id == userId))).Should().Be(1);
    }

    [Fact]
    public async Task Consumer_DeveEscutarFilaDoContrato_QuandoMensagemEnviadaParaNotificationsUserCreated()
    {
        var integrationEvent = NewEvent(Guid.CreateVersion7(), "Margaret Hamilton");

        await _fixture.WaitUntilConsumedAsync(await _fixture.SendToQueueAsync("notifications.user-created", integrationEvent));

        QueueNames.NotificationsUserCreated.Should().Be("notifications.user-created");
        _fixture.EmailSender.SentTo(integrationEvent.Email).Should().ContainSingle();
    }

    private static UserCreatedEvent NewEvent(Guid userId, string name)
        => new(Guid.CreateVersion7(), DateTime.UtcNow, userId, name, $"{userId:N}@fcg.dev");

    private Task<KnownUserModel?> FindKnownUserAsync(Guid userId)
        => _fixture.QueryAsync(context => context.Set<KnownUserModel>().AsNoTracking().SingleOrDefaultAsync(user => user.Id == userId));

    private Task<List<EmailLogModel>> EmailLogsAsync(Guid eventId)
        => _fixture.QueryAsync(context => context.Set<EmailLogModel>().AsNoTracking().Where(log => log.EventId == eventId).ToListAsync());
}
