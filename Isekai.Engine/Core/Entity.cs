using Isekai.Engine.Core.Component;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Exceptions;
using Isekai.Engine.Interfaces;

namespace Isekai.Engine.Core;

/// <summary>
/// Represents a world entity made of an identifier and a set of components.
/// </summary>
public sealed class Entity : IEntity
{
    private readonly Dictionary<Type, IComponent> _components = new();
    private readonly ITraceLogger _traceLogger;

    /// <summary>
    /// Creates an entity with the provided identifier.
    /// </summary>
    public Entity(EntityId id)
        : this(id, NoOpTraceLogger.Instance)
    {
    }

    /// <summary>
    /// Creates an entity with the provided identifier and diagnostic logger.
    /// </summary>
    public Entity(EntityId id, ITraceLogger traceLogger)
    {
        Id = id;
        _traceLogger = traceLogger ?? NoOpTraceLogger.Instance;

        using var trace = _traceLogger.BeginScope("Entity.ctor");
    }

    /// <inheritdoc />
    public EntityId Id { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<IComponent> Components => _components.Values.ToArray();

    /// <inheritdoc />
    public void AddComponent<TComponent>(TComponent component)
        where TComponent : IComponent
    {
        using var trace = _traceLogger.BeginScope("Entity.AddComponent");
        ArgumentNullException.ThrowIfNull(component);

        var componentType = typeof(TComponent);
        _traceLogger.Write($"ComponentType: {componentType.Name}");

        if (_components.ContainsKey(componentType))
        {
            throw new ComponentAlreadyExistsException(Id, componentType);
        }

        _components.Add(componentType, component);
    }

    /// <inheritdoc />
    public void SetComponent<TComponent>(TComponent component)
        where TComponent : IComponent
    {
        using var trace = _traceLogger.BeginScope("Entity.SetComponent");
        ArgumentNullException.ThrowIfNull(component);

        var componentType = typeof(TComponent);
        _traceLogger.Write($"ComponentType: {componentType.Name}");

        _components[componentType] = component;
    }

    /// <inheritdoc />
    public bool HasComponent<TComponent>()
        where TComponent : IComponent
    {
        using var trace = _traceLogger.BeginScope("Entity.HasComponent");
        return _components.ContainsKey(typeof(TComponent));
    }

    /// <inheritdoc />
    public TComponent GetComponent<TComponent>()
        where TComponent : IComponent
    {
        using var trace = _traceLogger.BeginScope("Entity.GetComponent");
        var componentType = typeof(TComponent);
        if (!_components.TryGetValue(componentType, out var component))
        {
            throw new ComponentNotFoundException(Id, componentType);
        }

        return (TComponent)component;
    }

    /// <inheritdoc />
    public bool TryGetComponent<TComponent>(out TComponent? component)
        where TComponent : IComponent
    {
        using var trace = _traceLogger.BeginScope("Entity.TryGetComponent");
        if (_components.TryGetValue(typeof(TComponent), out var value))
        {
            component = (TComponent)value;
            return true;
        }

        component = default;
        return false;
    }

    /// <inheritdoc />
    public bool RemoveComponent<TComponent>()
        where TComponent : IComponent
    {
        using var trace = _traceLogger.BeginScope("Entity.RemoveComponent");
        return _components.Remove(typeof(TComponent));
    }
}
