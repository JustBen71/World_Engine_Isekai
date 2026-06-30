namespace Isekai.Engine.Interfaces;

/// <summary>
/// Defines publish and subscribe operations for world events.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Subscribes a handler to an event type.
    /// </summary>
    IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        where TEvent : notnull;

    /// <summary>
    /// Publishes an event to all handlers registered for its type.
    /// </summary>
    void Publish<TEvent>(TEvent worldEvent)
        where TEvent : notnull;
}
