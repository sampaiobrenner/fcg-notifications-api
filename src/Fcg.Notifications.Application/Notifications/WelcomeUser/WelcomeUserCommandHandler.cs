using Fcg.Notifications.Application._Shared.Contexts;
using Fcg.Notifications.Application._Shared.Email;
using Fcg.Notifications.Application.Properties;
using Fcg.Notifications.Domain.EmailLogs;
using Fcg.Notifications.Domain.KnownUsers;
using MediatR;

namespace Fcg.Notifications.Application.Notifications.WelcomeUser;

/// <summary>
/// Mantem a projecao <c>known_users</c> (upsert) e envia o e-mail de boas-vindas uma unica vez por <c>EventId</c>.
/// </summary>
internal sealed class WelcomeUserCommandHandler : IRequestHandler<WelcomeUserCommand>
{
    private readonly IKnownUserRepository _knownUserRepository;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IEmailSender _emailSender;
    private readonly INotificationsDbContext _context;
    private readonly TimeProvider _timeProvider;

    public WelcomeUserCommandHandler(
        IKnownUserRepository knownUserRepository,
        IEmailLogRepository emailLogRepository,
        IEmailSender emailSender,
        INotificationsDbContext context,
        TimeProvider timeProvider)
    {
        _knownUserRepository = knownUserRepository;
        _emailLogRepository = emailLogRepository;
        _emailSender = emailSender;
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Handle(WelcomeUserCommand request, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var knownUser = await _knownUserRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (knownUser is null)
            _knownUserRepository.Add(KnownUser.Create(request.UserId, request.Name, request.Email, now));
        else
            knownUser.Update(request.Name, request.Email, now);

        if (!await _emailLogRepository.ExistsByEventIdAsync(request.EventId, cancellationToken))
        {
            var message = new EmailMessage(
                request.Email,
                ApplicationResources.EmailBoasVindasAssunto,
                string.Format(ApplicationResources.EmailBoasVindasCorpo, request.Name));

            await _emailSender.SendAsync(message, cancellationToken);

            _emailLogRepository.Add(EmailLog.Register(request.EventId, EmailKind.Welcome, message.To, message.Subject, now));
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
