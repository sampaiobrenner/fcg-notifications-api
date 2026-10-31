using MediatR;

namespace Fcg.Notifications.Application.Notifications.WelcomeUser;

public sealed record WelcomeUserCommand(Guid EventId, Guid UserId, string Name, string Email) : IRequest;
