using Fcg.Notifications.Domain._Shared.Models;

namespace Fcg.Notifications.Application._Shared.Contexts;

public interface INotificationsDbContext
{
    IQueryable<T> DataSet<T>() where T : PersistenceModelBase;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
