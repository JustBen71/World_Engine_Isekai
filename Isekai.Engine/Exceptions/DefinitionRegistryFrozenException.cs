namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when a mutation is attempted on a frozen definition registry.
/// </summary>
public sealed class DefinitionRegistryFrozenException : InvalidOperationException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public DefinitionRegistryFrozenException()
        : base("The definition registry is frozen and cannot be modified.")
    {
    }
}
