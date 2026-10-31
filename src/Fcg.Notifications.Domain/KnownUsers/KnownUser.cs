using Fcg.Notifications.Domain._Shared.Exceptions;
using Fcg.Notifications.Domain._Shared.Models;
using Fcg.Notifications.Domain.Properties;

namespace Fcg.Notifications.Domain.KnownUsers;

public sealed class KnownUser : DomainModelBase<KnownUserModel>, IAggregateRoot
{
    public KnownUser(KnownUserModel persistenceModel) : base(persistenceModel)
    {
    }

    public string Name => PersistenceModel.Name;

    public string Email => PersistenceModel.Email;

    public static KnownUser Create(Guid userId, string name, string email, DateTime now)
    {
        if (userId == Guid.Empty)
            throw new BusinessException(string.Format(DomainResources.CampoObrigatorio, nameof(KnownUserModel.Id)));

        var user = new KnownUser(new KnownUserModel { Id = userId });
        user.Update(name, email, now);
        return user;
    }

    public void Update(string name, string email, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException(string.Format(DomainResources.CampoObrigatorio, nameof(KnownUserModel.Name)));

        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessException(string.Format(DomainResources.CampoObrigatorio, nameof(KnownUserModel.Email)));

        PersistenceModel.Name = name.Trim();
        PersistenceModel.Email = email.Trim();
        PersistenceModel.UpdatedAt = now;
    }
}
