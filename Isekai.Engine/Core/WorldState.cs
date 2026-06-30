using Isekai.Engine.Core.System;
using Isekai.Engine.Exceptions;
using Isekai.Engine.Interfaces;

namespace Isekai.Engine.Core;

/// <summary>
/// Represents the complete state of a simulated world and drives its tick loop.
/// </summary>
public sealed class WorldState : IWorldState
{
    private readonly Dictionary<EntityId, Entity> _entities = new();
    private readonly List<IWorldSystem> _systems = new();

    /// <summary>
    /// Creates an empty world state.
    /// </summary>
    public WorldState()
        : this(new SimulationTime(), new EventBus())
    {
    }

    /// <summary>
    /// Creates a world state with explicit core services.
    /// </summary>
    public WorldState(SimulationTime time, EventBus eventBus)
    {
        Time = time ?? throw new ArgumentNullException(nameof(time));
        EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    /// <inheritdoc />
    public IReadOnlyCollection<Entity> Entities => _entities.Values.ToArray();

    /// <inheritdoc />
    public IReadOnlyList<IWorldSystem> Systems => _systems.AsReadOnly();

    /// <inheritdoc />
    public SimulationTime Time { get; }

    /// <inheritdoc />
    public EventBus EventBus { get; }

    /// <inheritdoc />
    public Entity CreateEntity()
    {
        var entity = new Entity(EntityId.New());
        _entities.Add(entity.Id, entity);
        return entity;
    }

    /// <inheritdoc />
    public bool ContainsEntity(EntityId entityId)
    {
        return _entities.ContainsKey(entityId);
    }

    /// <inheritdoc />
    public Entity GetEntity(EntityId entityId)
    {
        if (!_entities.TryGetValue(entityId, out var entity))
        {
            throw new EntityNotFoundException(entityId);
        }

        return entity;
    }

    /// <inheritdoc />
    public bool TryGetEntity(EntityId entityId, out Entity? entity)
    {
        return _entities.TryGetValue(entityId, out entity);
    }

    /// <inheritdoc />
    public bool RemoveEntity(EntityId entityId)
    {
        return _entities.Remove(entityId);
    }

    /// <inheritdoc />
    public void RegisterSystem(IWorldSystem system)
    {
        ArgumentNullException.ThrowIfNull(system);
        _systems.Add(system);
    }

    /// <inheritdoc />
    public TickResult Tick(TimeSpan deltaTime)
    {
        if (deltaTime < TimeSpan.Zero)
        {
            throw new InvalidTickDeltaException(deltaTime);
        }

        Time.Advance(deltaTime);

        foreach (var system in _systems)
        {
            var context = new WorldSystemExecutionContext(this, deltaTime, Time, EventBus);
            system.Execute(context);
        }

        return new TickResult(deltaTime, Time.Elapsed, Time.TickCount, _systems.Count);
    }
}
