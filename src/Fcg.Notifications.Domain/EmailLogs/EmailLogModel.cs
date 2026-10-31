using Fcg.Notifications.Domain._Shared.Models;

namespace Fcg.Notifications.Domain.EmailLogs;

/// <summary>
/// Registro de um e-mail enviado (tabela <c>email_log</c>). O <see cref="EventId"/> e unico e garante
/// a idempotencia dos consumidores por evento de integracao.
/// </summary>
public sealed class EmailLogModel : PersistenceModelBase
{
    public const int KindMaxLength = 40;
    public const int ToEmailMaxLength = 320;
    public const int SubjectMaxLength = 200;

    public Guid EventId { get; set; }
    public EmailKind Kind { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
