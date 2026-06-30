namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when a system registration is attempted while the world is ticking.
/// </summary>
public sealed class SystemRegistrationDuringTickException : InvalidOperationException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public SystemRegistrationDuringTickException()
        : base("Systems cannot be registered while the world is executing a tick.")
    {
    }
}
