using Fcg.Notifications.Application.Properties;

namespace Fcg.Notifications.Application._Shared.Exceptions;

/// <summary>
/// O usuario ainda nao chegou na projecao <c>known_users</c> (o <c>PaymentProcessedEvent</c> chegou antes do
/// <c>UserCreatedEvent</c>). Nao herda de <c>BusinessException</c> de proposito: entra na politica de retry
/// (1s/5s/15s/30s) do MassTransit e e reprocessada ate a projecao existir.
/// </summary>
public sealed class UserNotKnownYetException : Exception
{
    public UserNotKnownYetException(Guid userId)
        : base(string.Format(ApplicationResources.UsuarioAindaNaoConhecido, userId))
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}
