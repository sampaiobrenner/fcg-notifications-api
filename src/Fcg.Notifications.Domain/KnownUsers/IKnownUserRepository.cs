namespace Fcg.Notifications.Domain.KnownUsers;

public interface IKnownUserRepository
{
    Task<KnownUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    void Add(KnownUser knownUser);
}
