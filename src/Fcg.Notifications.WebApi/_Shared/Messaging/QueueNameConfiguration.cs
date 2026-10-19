namespace Fcg.Notifications.WebApi._Shared.Messaging;

internal static class QueueNameConfiguration
{
    public static string GetQueueName(IConfiguration configuration, string queueNameKey, string defaultName)
    {
        var configuredName = configuration[$"Messaging:Queues:{queueNameKey}"];
        return string.IsNullOrWhiteSpace(configuredName) ? defaultName : configuredName;
    }
}
