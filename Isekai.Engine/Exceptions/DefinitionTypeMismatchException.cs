using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when a definition exists but does not match the requested type.
/// </summary>
public sealed class DefinitionTypeMismatchException : InvalidCastException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public DefinitionTypeMismatchException(DefinitionId definitionId, Type expectedType, Type actualType)
        : base($"Definition '{definitionId}' is of type '{actualType.Name}', not '{expectedType.Name}'.")
    {
        DefinitionId = definitionId;
        ExpectedType = expectedType;
        ActualType = actualType;
    }

    /// <summary>
    /// Gets the requested definition identifier.
    /// </summary>
    public DefinitionId DefinitionId { get; }

    /// <summary>
    /// Gets the expected definition type.
    /// </summary>
    public Type ExpectedType { get; }

    /// <summary>
    /// Gets the actual registered definition type.
    /// </summary>
    public Type ActualType { get; }
}
