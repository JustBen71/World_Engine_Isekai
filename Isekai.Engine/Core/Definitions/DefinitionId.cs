namespace Isekai.Engine.Core.Definitions;

/// <summary>
/// Identifies a data definition independently from runtime entities.
/// </summary>
public readonly record struct DefinitionId
{
    /// <summary>
    /// Creates a definition identifier from a stable string value.
    /// </summary>
    public DefinitionId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>
    /// Gets the stable definition identifier value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a definition identifier from a stable string value.
    /// </summary>
    public static DefinitionId From(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
