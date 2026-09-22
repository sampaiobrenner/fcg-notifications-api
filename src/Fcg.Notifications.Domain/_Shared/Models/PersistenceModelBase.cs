namespace Fcg.Notifications.Domain._Shared.Models;

public abstract class PersistenceModelBase
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
}
