using Fcg.Notifications.Domain.KnownUsers;
using Fcg.Notifications.Infrastructure._Shared.Context;

namespace Fcg.Notifications.Infrastructure.KnownUsers;

internal sealed class KnownUserRepository : IKnownUserRepository
{
    private readonly NotificationsDbContext _context;

    public KnownUserRepository(NotificationsDbContext context) => _context = context;

    public async Task<KnownUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var model = await _context.Set<KnownUserModel>().FindAsync([userId], cancellationToken);
        return model is null ? null : new KnownUser(model);
    }

    public void Add(KnownUser knownUser) => _context.Set<KnownUserModel>().Add(knownUser.PersistenceModel);
}
