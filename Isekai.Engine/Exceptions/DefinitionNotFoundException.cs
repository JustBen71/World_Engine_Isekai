using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when a definition identifier is not registered.
/// </summary>
public sealed class DefinitionNotFoundException : KeyNotFoundException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public DefinitionNotFoundException(DefinitionId definitionId)
        : base($"Definition '{definitionId}' was not found.")
    {
        DefinitionId = definitionId;
    }

    /// <summary>
    /// Gets the missing definition identifier.
    /// </summary>
    public DefinitionId DefinitionId { get; }
}
