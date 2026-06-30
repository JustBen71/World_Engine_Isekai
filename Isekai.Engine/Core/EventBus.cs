using Isekai.Engine.Interfaces;

namespace Isekai.Engine.Core;

/// <summary>
/// Dispatches events between systems without containing simulation rules.
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    /// <inheritdoc />
    public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        where TEvent : notnull
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(TEvent);
        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers = new List<Delegate>();
            _handlers.Add(eventType, handlers);
        }

        handlers.Add(handler);
        return new Subscription(() => handlers.Remove(handler));
    }

    /// <inheritdoc />
    public void Publish<TEvent>(TEvent worldEvent)
        where TEvent : notnull
    {
        ArgumentNullException.ThrowIfNull(worldEvent);

        if (!_handlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            return;
        }

        foreach (var handler in handlers.ToArray())
        {
            ((Action<TEvent>)handler).Invoke(worldEvent);
        }
    }

    /// <summary>
    /// Represents a disposable subscription to an event handler list.
    /// </summary>
    private sealed class Subscription : IDisposable
    {
        private readonly Action _unsubscribe;
        private bool _isDisposed;

        public Subscription(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _unsubscribe();
            _isDisposed = true;
        }
    }
}
