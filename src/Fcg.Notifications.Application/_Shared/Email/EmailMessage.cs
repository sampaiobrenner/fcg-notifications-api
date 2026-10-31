namespace Fcg.Notifications.Application._Shared.Email;

public sealed record EmailMessage(string To, string Subject, string Body);
