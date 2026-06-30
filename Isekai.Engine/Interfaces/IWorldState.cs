using Isekai.Engine.Core;
using Isekai.Engine.Core.Component;
using Isekai.Engine.Core.Definitions;
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
    /// Gets the data definitions available to this world.
    /// </summary>
    DefinitionRegistry Definitions { get; }

    /// <summary>
    /// Returns true while the world is executing a tick.
    /// Structural entity mutations requested during this period are deferred until the tick ends.
    /// </summary>
    bool IsTicking { get; }

    /// <summary>
    /// Creates and registers a new entity.
    /// When called during a tick, the entity is created immediately but registered after all systems finish.
    /// </summary>
    Entity CreateEntity();

    /// <summary>
    /// Gets all entities that own a component of the requested type.
    /// </summary>
    IReadOnlyCollection<Entity> EntitiesWith<TComponent>()
        where TComponent : IComponent;

    /// <summary>
    /// Gets all entities that own both requested component types.
    /// </summary>
    IReadOnlyCollection<Entity> EntitiesWith<TComponentA, TComponentB>()
        where TComponentA : IComponent
        where TComponentB : IComponent;

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
    /// When called during a tick, the removal is deferred until after all systems finish.
    /// </summary>
    bool RemoveEntity(EntityId entityId);

    /// <summary>
    /// Registers a system to be executed on each tick.
    /// Systems must be registered outside the tick loop.
    /// </summary>
    void RegisterSystem(IWorldSystem system);

    /// <summary>
    /// Executes one deterministic simulation tick.
    /// Systems run in registration order and observe a stable entity set for the whole tick.
    /// </summary>
    TickResult Tick(TimeSpan deltaTime);
}
