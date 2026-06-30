namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when a required JSON definition field is missing.
/// </summary>
public sealed class MissingDefinitionFieldException : InvalidDefinitionDataException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public MissingDefinitionFieldException(string fieldName, string sourceName)
        : base($"Definition in '{sourceName}' is missing required field '{fieldName}'.")
    {
        FieldName = fieldName;
        SourceName = sourceName;
    }

    /// <summary>
    /// Gets the missing field name.
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    /// Gets the source name or file path that contains the invalid definition.
    /// </summary>
    public string SourceName { get; }
}
