using Fcg.Notifications.Application._Shared.Email;
using Fcg.Notifications.Infrastructure.Email;
using Fcg.Notifications.UnitTests._Fakes;
using Microsoft.Extensions.Logging;

namespace Fcg.Notifications.UnitTests.Infrastructure.Email;

public class ConsoleEmailSenderTests
{
    [Fact]
    public async Task SendAsync_DeveGravarLogEstruturado_QuandoEmailEnviado()
    {
        var logger = new RecordingLogger<ConsoleEmailSender>();
        var sender = new ConsoleEmailSender(logger);

        await sender.SendAsync(new EmailMessage("ada@fcg.dev", "Assunto", "Corpo"), CancellationToken.None);

        var entry = logger.Entries.Should().ContainSingle().Subject;
        entry.Level.Should().Be(LogLevel.Information);
        entry.Message.Should().Be("E-mail enviado para ada@fcg.dev | Assunto: Assunto | Corpo");
        entry.Properties.Should().Contain("To", "ada@fcg.dev")
            .And.Contain("Subject", "Assunto")
            .And.Contain("Body", "Corpo")
            .And.Contain("{OriginalFormat}", "E-mail enviado para {To} | Assunto: {Subject} | {Body}");
    }

    [Fact]
    public async Task SendAsync_DeveLancarExcecao_QuandoMensagemNula()
    {
        var sender = new ConsoleEmailSender(new RecordingLogger<ConsoleEmailSender>());

        var act = () => sender.SendAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
