using Isekai.Engine.Diagnostics;
using Isekai.Engine.Interfaces;

namespace Isekai.Engine.Core;

/// <summary>
/// Stores the current simulation time.
/// </summary>
public sealed class SimulationTime : ISimulationTime
{
    private readonly ITraceLogger _traceLogger;

    /// <summary>
    /// Creates a simulation clock without diagnostic output.
    /// </summary>
    public SimulationTime()
        : this(NoOpTraceLogger.Instance)
    {
    }

    /// <summary>
    /// Creates a simulation clock with a diagnostic logger.
    /// </summary>
    public SimulationTime(ITraceLogger traceLogger)
    {
        _traceLogger = traceLogger ?? NoOpTraceLogger.Instance;

        using var trace = _traceLogger.BeginScope("SimulationTime.ctor");
    }

    /// <inheritdoc />
    public TimeSpan Elapsed { get; private set; } = TimeSpan.Zero;

    /// <inheritdoc />
    public TimeSpan Delta { get; private set; } = TimeSpan.Zero;

    /// <inheritdoc />
    public ulong TickCount { get; private set; }

    /// <summary>
    /// Advances the simulation clock by one tick.
    /// </summary>
    public void Advance(TimeSpan deltaTime)
    {
        using var trace = _traceLogger.BeginScope("SimulationTime.Advance");
        Delta = deltaTime;
        Elapsed += deltaTime;
        TickCount++;
    }
}
