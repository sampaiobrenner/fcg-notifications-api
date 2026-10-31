using Fcg.Notifications.Domain._Shared.Exceptions;
using Fcg.Notifications.Domain.EmailLogs;

namespace Fcg.Notifications.UnitTests.Domain.EmailLogs;

public class EmailLogTests
{
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Register_DevePreencherRegistro_QuandoDadosValidos()
    {
        var eventId = Guid.CreateVersion7();

        var log = EmailLog.Register(eventId, EmailKind.Welcome, "ada@fcg.dev", "Assunto", Now);

        log.PersistenceModel.Should().BeEquivalentTo(new
        {
            EventId = eventId,
            Kind = EmailKind.Welcome,
            ToEmail = "ada@fcg.dev",
            Subject = "Assunto",
            SentAt = Now
        });
        log.Id().Should().NotBeEmpty();
    }

    [Fact]
    public void Register_DeveLancarBusinessException_QuandoEventIdVazio()
    {
        var act = () => EmailLog.Register(Guid.Empty, EmailKind.Welcome, "ada@fcg.dev", "Assunto", Now);

        act.Should().Throw<BusinessException>();
    }

    [Theory]
    [InlineData("", "Assunto")]
    [InlineData("ada@fcg.dev", "")]
    public void Register_DeveLancarBusinessException_QuandoCampoObrigatorioVazio(string toEmail, string subject)
    {
        var act = () => EmailLog.Register(Guid.CreateVersion7(), EmailKind.PurchaseConfirmed, toEmail, subject, Now);

        act.Should().Throw<BusinessException>();
    }
}
