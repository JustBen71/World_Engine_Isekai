using Isekai.Engine.Interfaces;

namespace Isekai.Engine.Core.System;

/// <summary>
/// Provides the minimal world services available to a system during execution.
/// </summary>
public sealed class WorldSystemExecutionContext
{
    /// <summary>
    /// Creates a context for one system execution.
    /// </summary>
    public WorldSystemExecutionContext(IWorldState world, TimeSpan deltaTime, ISimulationTime time, IEventBus eventBus)
    {
        World = world;
        DeltaTime = deltaTime;
        Time = time;
        EventBus = eventBus;
    }

    /// <summary>
    /// Gets the world state being simulated.
    /// </summary>
    public IWorldState World { get; }

    /// <summary>
    /// Gets the delta time used by the current tick.
    /// </summary>
    public TimeSpan DeltaTime { get; }

    /// <summary>
    /// Gets the simulation clock.
    /// </summary>
    public ISimulationTime Time { get; }

    /// <summary>
    /// Gets the event bus shared by world systems.
    /// </summary>
    public IEventBus EventBus { get; }
}
