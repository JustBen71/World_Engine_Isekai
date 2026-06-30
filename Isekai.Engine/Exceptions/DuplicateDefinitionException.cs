using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when a definition identifier is registered more than once.
/// </summary>
public sealed class DuplicateDefinitionException : InvalidOperationException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public DuplicateDefinitionException(DefinitionId definitionId)
        : base($"Definition '{definitionId}' is already registered.")
    {
        DefinitionId = definitionId;
    }

    /// <summary>
    /// Gets the duplicated definition identifier.
    /// </summary>
    public DefinitionId DefinitionId { get; }
}
