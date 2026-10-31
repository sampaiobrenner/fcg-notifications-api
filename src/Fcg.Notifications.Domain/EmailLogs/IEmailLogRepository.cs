namespace Fcg.Notifications.Domain.EmailLogs;

public interface IEmailLogRepository
{
    Task<bool> ExistsByEventIdAsync(Guid eventId, CancellationToken cancellationToken);

    void Add(EmailLog emailLog);
}
