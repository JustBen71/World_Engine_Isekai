namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when a definition data file does not exist.
/// </summary>
public sealed class DefinitionDataFileNotFoundException : FileNotFoundException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public DefinitionDataFileNotFoundException(string filePath)
        : base($"Definition data file '{filePath}' was not found.", filePath)
    {
    }
}
