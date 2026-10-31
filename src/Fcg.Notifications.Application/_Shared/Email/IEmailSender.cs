namespace Fcg.Notifications.Application._Shared.Email;

/// <summary>
/// Porta de saida para envio de e-mails. Na Fase 2 o envio e simulado (log estruturado no console).
/// </summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
