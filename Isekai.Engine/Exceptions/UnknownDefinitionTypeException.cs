namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when JSON references a definition type that was not registered.
/// </summary>
public sealed class UnknownDefinitionTypeException : InvalidDefinitionDataException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public UnknownDefinitionTypeException(string typeName)
        : base($"Definition type '{typeName}' is not registered.")
    {
        TypeName = typeName;
    }

    /// <summary>
    /// Gets the unknown definition type name.
    /// </summary>
    public string TypeName { get; }
}
