using Isekai.Engine.Core;
using Isekai.Engine.Core.System;

namespace Isekai.Engine.Interfaces;

/// <summary>
/// Defines the public contract of a world state.
/// </summary>
public interface IWorldState
{
    /// <summary>
    /// Gets all entities currently present in the world.
    /// </summary>
    IReadOnlyCollection<Entity> Entities { get; }

    /// <summary>
    /// Gets all systems registered in deterministic execution order.
    /// </summary>
    IReadOnlyList<IWorldSystem> Systems { get; }

    /// <summary>
    /// Gets the simulation clock owned by the world.
    /// </summary>
    SimulationTime Time { get; }

    /// <summary>
    /// Gets the event bus owned by the world.
    /// </summary>
    EventBus EventBus { get; }

    /// <summary>
    /// Creates and registers a new entity.
    /// </summary>
    Entity CreateEntity();

    /// <summary>
    /// Returns true when the world contains the requested entity.
    /// </summary>
    bool ContainsEntity(EntityId entityId);

    /// <summary>
    /// Gets an entity by identifier.
    /// </summary>
    Entity GetEntity(EntityId entityId);

    /// <summary>
    /// Tries to get an entity by identifier.
    /// </summary>
    bool TryGetEntity(EntityId entityId, out Entity? entity);

    /// <summary>
    /// Removes an entity from the world.
    /// </summary>
    bool RemoveEntity(EntityId entityId);

    /// <summary>
    /// Registers a system to be executed on each tick.
    /// </summary>
    void RegisterSystem(IWorldSystem system);

    /// <summary>
    /// Executes one deterministic simulation tick.
    /// </summary>
    TickResult Tick(TimeSpan deltaTime);
}
