using System.Collections.Concurrent;
using Fcg.Notifications.Application._Shared.Email;

namespace Fcg.Notifications.IntegrationTests._Fixtures;

/// <summary>
/// Substitui o <c>ConsoleEmailSender</c> nos testes, guardando os e-mails "enviados" para asserts.
/// </summary>
public sealed class RecordingEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<EmailMessage> _messages = new();

    public IReadOnlyCollection<EmailMessage> SentTo(string to) => _messages.Where(message => message.To == to).ToList();

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        _messages.Enqueue(message);
        return Task.CompletedTask;
    }
}
