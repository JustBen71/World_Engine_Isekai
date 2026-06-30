using Isekai.Engine.Interfaces;

namespace Isekai.Engine.Core;

/// <summary>
/// Stores the current simulation time.
/// </summary>
public sealed class SimulationTime : ISimulationTime
{
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
        Delta = deltaTime;
        Elapsed += deltaTime;
        TickCount++;
    }
}
