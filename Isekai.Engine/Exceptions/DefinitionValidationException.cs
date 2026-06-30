using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when one or more registered definitions are invalid.
/// </summary>
public sealed class DefinitionValidationException : InvalidDefinitionDataException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public DefinitionValidationException(IReadOnlyCollection<DefinitionValidationError> errors)
        : base($"Definition validation failed with {errors.Count} error(s).")
    {
        Errors = errors;
    }

    /// <summary>
    /// Gets every validation error found.
    /// </summary>
    public IReadOnlyCollection<DefinitionValidationError> Errors { get; }
}
