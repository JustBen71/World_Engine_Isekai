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
    /// Publishes an event to handlers registered for its type.
    /// The owning world may queue the event during a tick and dispatch it at the deterministic end-of-tick phase.
    /// </summary>
    void Publish<TEvent>(TEvent worldEvent)
        where TEvent : notnull;
}
