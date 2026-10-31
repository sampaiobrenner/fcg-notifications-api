using Fcg.Notifications.Domain.EmailLogs;
using Fcg.Notifications.Infrastructure._Shared.Context;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Notifications.Infrastructure.EmailLogs;

internal sealed class EmailLogRepository : IEmailLogRepository
{
    private readonly NotificationsDbContext _context;

    public EmailLogRepository(NotificationsDbContext context) => _context = context;

    public Task<bool> ExistsByEventIdAsync(Guid eventId, CancellationToken cancellationToken)
        => _context.Set<EmailLogModel>().AnyAsync(log => log.EventId == eventId, cancellationToken);

    public void Add(EmailLog emailLog) => _context.Set<EmailLogModel>().Add(emailLog.PersistenceModel);
}
