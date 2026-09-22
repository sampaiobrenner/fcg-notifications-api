using Fcg.Notifications.Application.Properties;

namespace Fcg.Notifications.Application._Shared.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string resource, object key)
        : base(string.Format(ApplicationResources.RecursoNaoEncontrado, resource, key))
    {
    }
}
