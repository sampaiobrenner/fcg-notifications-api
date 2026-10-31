using Fcg.Notifications.Domain._Shared.Exceptions;
using Fcg.Notifications.Domain._Shared.Models;
using Fcg.Notifications.Domain.Properties;

namespace Fcg.Notifications.Domain.EmailLogs;

public sealed class EmailLog : DomainModelBase<EmailLogModel>, IAggregateRoot
{
    public EmailLog(EmailLogModel persistenceModel) : base(persistenceModel)
    {
    }

    public static EmailLog Register(Guid eventId, EmailKind kind, string toEmail, string subject, DateTime sentAt)
    {
        if (eventId == Guid.Empty)
            throw new BusinessException(string.Format(DomainResources.CampoObrigatorio, nameof(EmailLogModel.EventId)));

        if (string.IsNullOrWhiteSpace(toEmail))
            throw new BusinessException(string.Format(DomainResources.CampoObrigatorio, nameof(EmailLogModel.ToEmail)));

        if (string.IsNullOrWhiteSpace(subject))
            throw new BusinessException(string.Format(DomainResources.CampoObrigatorio, nameof(EmailLogModel.Subject)));

        return new EmailLog(new EmailLogModel
        {
            EventId = eventId,
            Kind = kind,
            ToEmail = toEmail,
            Subject = subject,
            SentAt = sentAt
        });
    }
}
