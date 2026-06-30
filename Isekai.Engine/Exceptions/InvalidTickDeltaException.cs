namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when a tick receives an invalid delta time.
/// </summary>
public sealed class InvalidTickDeltaException : ArgumentOutOfRangeException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public InvalidTickDeltaException(TimeSpan deltaTime)
        : base(nameof(deltaTime), deltaTime, "Tick delta time cannot be negative.")
    {
    }
}
