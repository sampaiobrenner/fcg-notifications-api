using Fcg.Notifications.Application._Shared.Email;
using Microsoft.Extensions.Logging;

namespace Fcg.Notifications.Infrastructure.Email;

/// <summary>
/// Simula o envio de e-mail gravando um log estruturado (Serilog) com destinatario, assunto e corpo. Nao usa SMTP.
/// </summary>
internal sealed class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) => _logger = logger;

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        _logger.LogInformation("E-mail enviado para {To} | Assunto: {Subject} | {Body}", message.To, message.Subject, message.Body);

        return Task.CompletedTask;
    }
}
