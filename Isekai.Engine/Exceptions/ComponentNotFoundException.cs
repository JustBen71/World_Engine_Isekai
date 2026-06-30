using Isekai.Engine.Core;

namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when an entity does not own a requested component type.
/// </summary>
public sealed class ComponentNotFoundException : KeyNotFoundException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public ComponentNotFoundException(EntityId entityId, Type componentType)
        : base($"Entity '{entityId}' does not have a component of type '{componentType.Name}'.")
    {
        EntityId = entityId;
        ComponentType = componentType;
    }

    /// <summary>
    /// Gets the entity missing the component.
    /// </summary>
    public EntityId EntityId { get; }

    /// <summary>
    /// Gets the missing component type.
    /// </summary>
    public Type ComponentType { get; }
}
