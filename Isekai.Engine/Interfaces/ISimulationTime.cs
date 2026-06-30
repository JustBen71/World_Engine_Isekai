namespace Isekai.Engine.Interfaces;

/// <summary>
/// Defines read-only access to the simulation clock.
/// </summary>
public interface ISimulationTime
{
    /// <summary>
    /// Gets the total simulated time elapsed since world creation.
    /// </summary>
    TimeSpan Elapsed { get; }

    /// <summary>
    /// Gets the delta time used by the latest tick.
    /// </summary>
    TimeSpan Delta { get; }

    /// <summary>
    /// Gets the number of ticks executed by the world.
    /// </summary>
    ulong TickCount { get; }
}
