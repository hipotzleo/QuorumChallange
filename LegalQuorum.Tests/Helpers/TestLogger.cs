using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace LegalQuorum.Tests.Helpers;

public sealed class TestLogger<T> : ILogger<T>, IDisposable
{
    private readonly ConcurrentQueue<(LogLevel level, string message, Exception? ex)> _logs = new();

    public IReadOnlyCollection<(LogLevel level, string message, Exception? ex)> Logs => _logs.ToArray();

    public IDisposable BeginScope<TState>(TState state) => this;
    public void Dispose() { /* no-op */ }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId,
        TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _logs.Enqueue((logLevel, formatter(state, exception), exception));
    }

    public int CountWarnings() => _logs.Count(x => x.level == LogLevel.Warning);
    public int CountErrors() => _logs.Count(x => x.level == LogLevel.Error);
}
