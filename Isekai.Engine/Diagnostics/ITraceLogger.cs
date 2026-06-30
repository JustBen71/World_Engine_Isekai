namespace Isekai.Engine.Diagnostics;

/// <summary>
/// Writes structured diagnostic traces for engine execution.
/// </summary>
public interface ITraceLogger
{
    /// <summary>
    /// Gets the active log level.
    /// </summary>
    int Level { get; }

    /// <summary>
    /// Returns true when the logger accepts entries for the requested level.
    /// </summary>
    bool IsEnabled(int level);

    /// <summary>
    /// Starts a named trace block and writes its closing line when disposed.
    /// </summary>
    IDisposable BeginScope(string name, int level = EngineLogLevels.FullTrace);

    /// <summary>
    /// Writes a diagnostic message inside the current trace block.
    /// </summary>
    void Write(string message, int level = EngineLogLevels.FullTrace);
}
