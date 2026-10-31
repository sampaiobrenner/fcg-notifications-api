using Fcg.Notifications.Domain._Shared.Models;

namespace Fcg.Notifications.Domain.KnownUsers;

/// <summary>
/// Projecao local de um usuario da UsersAPI (tabela <c>known_users</c>). O <see cref="PersistenceModelBase.Id"/>
/// e o proprio <c>UserId</c> do evento e e mapeado para a coluna <c>user_id</c>.
/// </summary>
public sealed class KnownUserModel : PersistenceModelBase
{
    public const int NameMaxLength = 120;
    public const int EmailMaxLength = 320;

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
