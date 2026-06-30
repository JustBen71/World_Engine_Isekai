namespace Isekai.Engine.Diagnostics;

/// <summary>
/// Trace logger that ignores all diagnostic entries.
/// </summary>
public sealed class NoOpTraceLogger : ITraceLogger
{
    /// <summary>
    /// Gets the shared no-op logger instance.
    /// </summary>
    public static NoOpTraceLogger Instance { get; } = new();

    private NoOpTraceLogger()
    {
    }

    /// <inheritdoc />
    public int Level => EngineLogLevels.Off;

    /// <inheritdoc />
    public bool IsEnabled(int level) => false;

    /// <inheritdoc />
    public IDisposable BeginScope(string name, int level = EngineLogLevels.FullTrace)
    {
        return EmptyScope.Instance;
    }

    /// <inheritdoc />
    public void Write(string message, int level = EngineLogLevels.FullTrace)
    {
    }

    /// <summary>
    /// Disposable scope that performs no action.
    /// </summary>
    private sealed class EmptyScope : IDisposable
    {
        public static EmptyScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
