using System.Globalization;
using Fcg.Contracts.Events.V1;
using Fcg.Notifications.Application._Shared.Contexts;
using Fcg.Notifications.Application._Shared.Email;
using Fcg.Notifications.Application._Shared.Exceptions;
using Fcg.Notifications.Application.Properties;
using Fcg.Notifications.Domain.EmailLogs;
using Fcg.Notifications.Domain.KnownUsers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Notifications.Application.Notifications.NotifyPayment;

/// <summary>
/// <c>Rejected</c>: apenas log. <c>Approved</c>: envia a confirmacao de compra uma unica vez por <c>EventId</c>,
/// usando nome/e-mail da projecao <c>known_users</c>; usuario ausente lanca <see cref="UserNotKnownYetException"/> (retry).
/// </summary>
internal sealed class NotifyPaymentCommandHandler : IRequestHandler<NotifyPaymentCommand>
{
    private static readonly CultureInfo BrazilianCulture = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IKnownUserRepository _knownUserRepository;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IEmailSender _emailSender;
    private readonly INotificationsDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<NotifyPaymentCommandHandler> _logger;

    public NotifyPaymentCommandHandler(
        IKnownUserRepository knownUserRepository,
        IEmailLogRepository emailLogRepository,
        IEmailSender emailSender,
        INotificationsDbContext context,
        TimeProvider timeProvider,
        ILogger<NotifyPaymentCommandHandler> logger)
    {
        _knownUserRepository = knownUserRepository;
        _emailLogRepository = emailLogRepository;
        _emailSender = emailSender;
        _context = context;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task Handle(NotifyPaymentCommand request, CancellationToken cancellationToken)
    {
        if (request.Status == PaymentStatus.Rejected)
        {
            _logger.LogInformation("Pagamento {OrderId} rejeitado: {Reason}", request.OrderId, request.Reason);
            return;
        }

        var knownUser = await _knownUserRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new UserNotKnownYetException(request.UserId);

        if (await _emailLogRepository.ExistsByEventIdAsync(request.EventId, cancellationToken))
            return;

        var message = new EmailMessage(
            knownUser.Email,
            string.Format(BrazilianCulture, ApplicationResources.EmailCompraConfirmadaAssunto, request.OrderId),
            string.Format(BrazilianCulture, ApplicationResources.EmailCompraConfirmadaCorpo, knownUser.Name, request.Price));

        await _emailSender.SendAsync(message, cancellationToken);

        _emailLogRepository.Add(EmailLog.Register(
            request.EventId,
            EmailKind.PurchaseConfirmed,
            message.To,
            message.Subject,
            _timeProvider.GetUtcNow().UtcDateTime));

        await _context.SaveChangesAsync(cancellationToken);
    }
}
