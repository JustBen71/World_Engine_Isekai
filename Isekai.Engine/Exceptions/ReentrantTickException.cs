namespace Isekai.Engine.Exceptions;

/// <summary>
/// Thrown when a tick is started while another tick is already running.
/// </summary>
public sealed class ReentrantTickException : InvalidOperationException
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    public ReentrantTickException()
        : base("A world tick cannot be started while another tick is already running.")
    {
    }
}
