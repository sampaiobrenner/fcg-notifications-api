using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Fcg.Notifications.UnitTests._Fakes;

/// <summary>
/// Logger que guarda cada entrada com o template e as propriedades estruturadas, para asserts sobre logs.
/// </summary>
internal sealed class RecordingLogger<T> : ILogger<T>
{
    public ConcurrentQueue<LogEntry> Entries { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var properties = state as IReadOnlyList<KeyValuePair<string, object?>> ?? [];
        Entries.Enqueue(new LogEntry(logLevel, formatter(state, exception), properties.ToDictionary(pair => pair.Key, pair => pair.Value)));
    }
}

internal sealed record LogEntry(LogLevel Level, string Message, IReadOnlyDictionary<string, object?> Properties);
