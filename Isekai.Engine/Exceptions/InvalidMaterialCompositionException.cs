namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when a material composition is structurally invalid.
/// </summary>
public sealed class InvalidMaterialCompositionException : InvalidOperationException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public InvalidMaterialCompositionException(string message)
        : base(message)
    {
    }
}
