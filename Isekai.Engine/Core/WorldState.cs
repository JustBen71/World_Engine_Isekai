using Isekai.Engine.Core.Component;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Core.System;
using Isekai.Engine.Diagnostics;
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
    private readonly List<PendingWorldMutation> _pendingWorldMutations = new();
    private bool _isTicking;

    /// <summary>
    /// Creates an empty world state.
    /// </summary>
    public WorldState()
        : this(NoOpTraceLogger.Instance)
    {
    }

    /// <summary>
    /// Creates an empty world state with a diagnostic logger.
    /// </summary>
    public WorldState(ITraceLogger traceLogger)
        : this(new SimulationTime(traceLogger), new EventBus(traceLogger), new DefinitionRegistry(traceLogger), traceLogger)
    {
    }

    /// <summary>
    /// Creates a world state with explicit core services.
    /// </summary>
    public WorldState(SimulationTime time, EventBus eventBus)
        : this(time, eventBus, new DefinitionRegistry(), NoOpTraceLogger.Instance)
    {
    }

    /// <summary>
    /// Creates a world state with explicit core services and diagnostic logger.
    /// </summary>
    public WorldState(SimulationTime time, EventBus eventBus, ITraceLogger traceLogger)
        : this(time, eventBus, new DefinitionRegistry(traceLogger), traceLogger)
    {
    }

    /// <summary>
    /// Creates a world state with explicit core services, definitions and diagnostic logger.
    /// </summary>
    public WorldState(
        SimulationTime time,
        EventBus eventBus,
        DefinitionRegistry definitions,
        ITraceLogger traceLogger)
    {
        Time = time ?? throw new ArgumentNullException(nameof(time));
        EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        Definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        TraceLogger = traceLogger ?? NoOpTraceLogger.Instance;

        using var trace = TraceLogger.BeginScope("WorldState.ctor");
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
    public DefinitionRegistry Definitions { get; }

    /// <summary>
    /// Gets the diagnostic logger used by this world state.
    /// </summary>
    public ITraceLogger TraceLogger { get; }

    /// <inheritdoc />
    public bool IsTicking => _isTicking;

    /// <inheritdoc />
    public Entity CreateEntity()
    {
        using var trace = TraceLogger.BeginScope("WorldState.CreateEntity");

        var entity = new Entity(EntityId.New(), TraceLogger);

        if (_isTicking)
        {
            _pendingWorldMutations.Add(new PendingWorldMutation(
                $"CreateEntity({entity.Id})",
                () => _entities.Add(entity.Id, entity)));

            TraceLogger.Write($"Deferred: CreateEntity({entity.Id})");
            return entity;
        }

        _entities.Add(entity.Id, entity);
        return entity;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<Entity> EntitiesWith<TComponent>()
        where TComponent : IComponent
    {
        using var trace = TraceLogger.BeginScope("WorldState.EntitiesWith<TComponent>");
        return _entities.Values
            .Where(entity => entity.HasComponent<TComponent>())
            .ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<Entity> EntitiesWith<TComponentA, TComponentB>()
        where TComponentA : IComponent
        where TComponentB : IComponent
    {
        using var trace = TraceLogger.BeginScope("WorldState.EntitiesWith<TComponentA,TComponentB>");
        return _entities.Values
            .Where(entity => entity.HasComponent<TComponentA>() && entity.HasComponent<TComponentB>())
            .ToArray();
    }

    /// <inheritdoc />
    public bool ContainsEntity(EntityId entityId)
    {
        using var trace = TraceLogger.BeginScope("WorldState.ContainsEntity");
        return _entities.ContainsKey(entityId);
    }

    /// <inheritdoc />
    public Entity GetEntity(EntityId entityId)
    {
        using var trace = TraceLogger.BeginScope("WorldState.GetEntity");
        if (!_entities.TryGetValue(entityId, out var entity))
        {
            throw new EntityNotFoundException(entityId);
        }

        return entity;
    }

    /// <inheritdoc />
    public bool TryGetEntity(EntityId entityId, out Entity? entity)
    {
        using var trace = TraceLogger.BeginScope("WorldState.TryGetEntity");
        return _entities.TryGetValue(entityId, out entity);
    }

    /// <inheritdoc />
    public bool RemoveEntity(EntityId entityId)
    {
        using var trace = TraceLogger.BeginScope("WorldState.RemoveEntity");

        if (_isTicking)
        {
            var exists = _entities.ContainsKey(entityId);
            if (exists)
            {
                _pendingWorldMutations.Add(new PendingWorldMutation(
                    $"RemoveEntity({entityId})",
                    () => _entities.Remove(entityId)));

                TraceLogger.Write($"Deferred: RemoveEntity({entityId})");
            }

            return exists;
        }

        return _entities.Remove(entityId);
    }

    /// <inheritdoc />
    public void RegisterSystem(IWorldSystem system)
    {
        using var trace = TraceLogger.BeginScope("WorldState.RegisterSystem");
        ArgumentNullException.ThrowIfNull(system);

        if (_isTicking)
        {
            throw new SystemRegistrationDuringTickException();
        }

        _systems.Add(system);
    }

    /// <inheritdoc />
    public TickResult Tick(TimeSpan deltaTime)
    {
        using var trace = TraceLogger.BeginScope("WorldState.Tick");

        if (_isTicking)
        {
            throw new ReentrantTickException();
        }

        if (deltaTime < TimeSpan.Zero)
        {
            throw new InvalidTickDeltaException(deltaTime);
        }

        var eventQueueStartedByTick = false;
        _isTicking = true;
        try
        {
            EventBus.BeginEventQueue();
            eventQueueStartedByTick = true;
            Time.Advance(deltaTime);

            foreach (var system in _systems)
            {
                using var systemTrace = TraceLogger.BeginScope($"System.Execute({system.GetType().Name})");
                var context = new WorldSystemExecutionContext(this, deltaTime, Time, EventBus);
                system.Execute(context);
            }

            EventBus.FlushQueuedEvents();
            ApplyPendingWorldMutations();
            return new TickResult(deltaTime, Time.Elapsed, Time.TickCount, _systems.Count);
        }
        catch
        {
            if (eventQueueStartedByTick)
            {
                EventBus.ClearQueuedEvents();
            }

            _pendingWorldMutations.Clear();
            throw;
        }
        finally
        {
            if (eventQueueStartedByTick)
            {
                EventBus.EndEventQueue();
            }

            _isTicking = false;
        }
    }

    private void ApplyPendingWorldMutations()
    {
        using var trace = TraceLogger.BeginScope("WorldState.ApplyPendingWorldMutations");

        foreach (var mutation in _pendingWorldMutations)
        {
            using var mutationTrace = TraceLogger.BeginScope($"WorldMutation.{mutation.Name}");
            mutation.Apply();
        }

        _pendingWorldMutations.Clear();
    }

    private sealed record PendingWorldMutation(string Name, Action Apply);
}
