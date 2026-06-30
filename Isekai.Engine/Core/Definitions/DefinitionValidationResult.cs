namespace Isekai.Engine.Core.Definitions;

/// <summary>
/// Contains the result of definition validation.
/// </summary>
public sealed class DefinitionValidationResult
{
    /// <summary>
    /// Creates a validation result.
    /// </summary>
    public DefinitionValidationResult(IReadOnlyCollection<DefinitionValidationError> errors)
    {
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
    }

    /// <summary>
    /// Gets all validation errors.
    /// </summary>
    public IReadOnlyCollection<DefinitionValidationError> Errors { get; }

    /// <summary>
    /// Returns true when no validation errors were found.
    /// </summary>
    public bool IsValid => Errors.Count == 0;
}
