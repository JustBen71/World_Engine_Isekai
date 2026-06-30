namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when a definition data directory does not exist.
/// </summary>
public sealed class DefinitionDataDirectoryNotFoundException : DirectoryNotFoundException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public DefinitionDataDirectoryNotFoundException(string directoryPath)
        : base($"Definition data directory '{directoryPath}' was not found.")
    {
        DirectoryPath = directoryPath;
    }

    /// <summary>
    /// Gets the missing directory path.
    /// </summary>
    public string DirectoryPath { get; }
}
