namespace Isekai.Engine.Core.Definitions;

/// <summary>
/// References another definition by stable identifier and expected definition type.
/// </summary>
public readonly record struct DefinitionReference<TDefinition>
    where TDefinition : IDefinition
{
    /// <summary>
    /// Creates a typed definition reference.
    /// </summary>
    public DefinitionReference(DefinitionId id)
    {
        if (string.IsNullOrWhiteSpace(id.Value))
        {
            throw new ArgumentException("Definition reference id cannot be empty.", nameof(id));
        }

        Id = id;
    }

    /// <summary>
    /// Gets the referenced definition identifier.
    /// </summary>
    public DefinitionId Id { get; }

    /// <summary>
    /// Creates a typed definition reference from a stable identifier value.
    /// </summary>
    public static DefinitionReference<TDefinition> From(string id) => new(DefinitionId.From(id));

    /// <inheritdoc />
    public override string ToString() => Id.ToString();
}
