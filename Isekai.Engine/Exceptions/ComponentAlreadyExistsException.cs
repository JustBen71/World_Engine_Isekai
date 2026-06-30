using Isekai.Engine.Core;

namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when an entity already owns a component of the requested type.
/// </summary>
public sealed class ComponentAlreadyExistsException : InvalidOperationException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public ComponentAlreadyExistsException(EntityId entityId, Type componentType)
        : base($"Entity '{entityId}' already has a component of type '{componentType.Name}'.")
    {
        EntityId = entityId;
        ComponentType = componentType;
    }

    /// <summary>
    /// Gets the entity that already owns the component.
    /// </summary>
    public EntityId EntityId { get; }

    /// <summary>
    /// Gets the duplicated component type.
    /// </summary>
    public Type ComponentType { get; }
}
