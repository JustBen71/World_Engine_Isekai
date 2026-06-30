using Isekai.Engine.Core;
using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Interfaces;

/// <summary>
/// Defines the public contract of an entity.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    EntityId Id { get; }

    /// <summary>
    /// Gets all components currently attached to the entity.
    /// </summary>
    IReadOnlyCollection<IComponent> Components { get; }

    /// <summary>
    /// Adds a component to the entity.
    /// </summary>
    void AddComponent<TComponent>(TComponent component)
        where TComponent : IComponent;

    /// <summary>
    /// Returns true when the entity has a component of the requested type.
    /// </summary>
    bool HasComponent<TComponent>()
        where TComponent : IComponent;

    /// <summary>
    /// Gets a component of the requested type.
    /// </summary>
    TComponent GetComponent<TComponent>()
        where TComponent : IComponent;

    /// <summary>
    /// Tries to get a component of the requested type.
    /// </summary>
    bool TryGetComponent<TComponent>(out TComponent? component)
        where TComponent : IComponent;

    /// <summary>
    /// Removes a component of the requested type.
    /// </summary>
    bool RemoveComponent<TComponent>()
        where TComponent : IComponent;
}
