namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when a definition JSON document cannot be parsed.
/// </summary>
public sealed class InvalidDefinitionJsonException : InvalidDefinitionDataException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public InvalidDefinitionJsonException(string sourceName, Exception innerException)
        : base($"Definition JSON '{sourceName}' is invalid.", innerException)
    {
        SourceName = sourceName;
    }

    /// <summary>
    /// Gets the source name or file path of the invalid JSON.
    /// </summary>
    public string SourceName { get; }

}
