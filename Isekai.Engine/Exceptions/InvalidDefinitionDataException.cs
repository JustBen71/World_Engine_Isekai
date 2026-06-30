namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when definition data is structurally invalid.
/// </summary>
public class InvalidDefinitionDataException : InvalidOperationException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public InvalidDefinitionDataException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates the exception with the underlying cause.
    /// </summary>
    public InvalidDefinitionDataException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
