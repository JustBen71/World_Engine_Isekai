namespace Isekai.Engine.Core.Definitions;

/// <summary>
/// Describes one validation error found in registered definitions.
/// </summary>
public sealed record DefinitionValidationError(
    DefinitionId SourceDefinitionId,
    string PropertyName,
    DefinitionId ReferencedDefinitionId,
    Type ExpectedDefinitionType,
    string Message);
