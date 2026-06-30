using Isekai.Engine.Core.Component;
using Isekai.Engine.Exceptions;
using Isekai.Engine.Interfaces;

namespace Isekai.Engine.Core;

/// <summary>
/// Represents a world entity made of an identifier and a set of components.
/// </summary>
public sealed class Entity : IEntity
{
    private readonly Dictionary<Type, IComponent> _components = new();

    /// <summary>
    /// Creates an entity with the provided identifier.
    /// </summary>
    public Entity(EntityId id)
    {
        Id = id;
    }

    /// <inheritdoc />
    public EntityId Id { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<IComponent> Components => _components.Values.ToArray();

    /// <inheritdoc />
    public void AddComponent<TComponent>(TComponent component)
        where TComponent : IComponent
    {
        ArgumentNullException.ThrowIfNull(component);

        var componentType = typeof(TComponent);
        if (_components.ContainsKey(componentType))
        {
            throw new ComponentAlreadyExistsException(Id, componentType);
        }

        _components.Add(componentType, component);
    }

    /// <inheritdoc />
    public bool HasComponent<TComponent>()
        where TComponent : IComponent
    {
        return _components.ContainsKey(typeof(TComponent));
    }

    /// <inheritdoc />
    public TComponent GetComponent<TComponent>()
        where TComponent : IComponent
    {
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
        return _components.Remove(typeof(TComponent));
    }
}
