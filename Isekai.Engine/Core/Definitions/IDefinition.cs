namespace Isekai.Engine.Core.Definitions;

/// <summary>
/// Defines the minimal contract for data-driven definitions.
/// Definitions describe data templates and must not contain simulation logic.
/// </summary>
public interface IDefinition
{
    /// <summary>
    /// Gets the stable identifier of this definition.
    /// </summary>
    DefinitionId Id { get; }
}
