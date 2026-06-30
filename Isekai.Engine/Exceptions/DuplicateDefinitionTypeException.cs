namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when a JSON definition type name is registered more than once.
/// </summary>
public sealed class DuplicateDefinitionTypeException : InvalidOperationException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public DuplicateDefinitionTypeException(string typeName)
        : base($"Definition type '{typeName}' is already registered.")
    {
        TypeName = typeName;
    }

    /// <summary>
    /// Gets the duplicated type name.
    /// </summary>
    public string TypeName { get; }
}
