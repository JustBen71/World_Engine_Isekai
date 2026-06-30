using Isekai.Engine.Core;

namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when a world does not contain a requested entity.
/// </summary>
public sealed class EntityNotFoundException : KeyNotFoundException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public EntityNotFoundException(EntityId entityId)
        : base($"Entity '{entityId}' was not found in the world.")
    {
        EntityId = entityId;
    }

    /// <summary>
    /// Gets the missing entity identifier.
    /// </summary>
    public EntityId EntityId { get; }
}
