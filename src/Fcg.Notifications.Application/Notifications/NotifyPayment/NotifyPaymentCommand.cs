using Fcg.Contracts.Events.V1;
using MediatR;

namespace Fcg.Notifications.Application.Notifications.NotifyPayment;

public sealed record NotifyPaymentCommand(
    Guid EventId,
    Guid OrderId,
    Guid UserId,
    decimal Price,
    PaymentStatus Status,
    string? Reason) : IRequest;
