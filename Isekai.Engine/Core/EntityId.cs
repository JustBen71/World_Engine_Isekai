namespace Isekai.Engine.Core;

/// <summary>
/// Identifies an entity inside a world state.
/// </summary>
public readonly record struct EntityId(Guid Value)
{
    /// <summary>
    /// Creates a new unique entity identifier.
    /// </summary>
    public static EntityId New() => new(Guid.NewGuid());

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
