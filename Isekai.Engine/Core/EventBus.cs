using Isekai.Engine.Diagnostics;
using Isekai.Engine.Interfaces;

namespace Isekai.Engine.Core;

/// <summary>
/// Dispatches events between systems without containing simulation rules.
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();
    private readonly Queue<QueuedEvent> _queuedEvents = new();
    private readonly ITraceLogger _traceLogger;
    private bool _isQueueingEvents;

    /// <summary>
    /// Creates an event bus without diagnostic output.
    /// </summary>
    public EventBus()
        : this(NoOpTraceLogger.Instance)
    {
    }

    /// <summary>
    /// Creates an event bus with a diagnostic logger.
    /// </summary>
    public EventBus(ITraceLogger traceLogger)
    {
        _traceLogger = traceLogger ?? NoOpTraceLogger.Instance;

        using var trace = _traceLogger.BeginScope("EventBus.ctor");
    }

    /// <summary>
    /// Returns true when published events are queued instead of dispatched immediately.
    /// </summary>
    public bool IsQueueingEvents => _isQueueingEvents;

    /// <summary>
    /// Gets the number of events currently waiting for dispatch.
    /// </summary>
    public int QueuedEventCount => _queuedEvents.Count;

    /// <inheritdoc />
    public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        where TEvent : notnull
    {
        using var trace = _traceLogger.BeginScope("EventBus.Subscribe");
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(TEvent);
        _traceLogger.Write($"EventType: {eventType.Name}");

        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers = new List<Delegate>();
            _handlers.Add(eventType, handlers);
        }

        handlers.Add(handler);
        return new Subscription(() => handlers.Remove(handler), _traceLogger);
    }

    /// <inheritdoc />
    public void Publish<TEvent>(TEvent worldEvent)
        where TEvent : notnull
    {
        using var trace = _traceLogger.BeginScope("EventBus.Publish");
        ArgumentNullException.ThrowIfNull(worldEvent);

        var eventType = typeof(TEvent);
        _traceLogger.Write($"EventType: {eventType.Name}");

        if (!_handlers.ContainsKey(eventType))
        {
            return;
        }

        if (_isQueueingEvents)
        {
            _queuedEvents.Enqueue(new QueuedEvent(eventType, worldEvent));
            _traceLogger.Write($"QueuedEventCount: {_queuedEvents.Count}");
            return;
        }

        Dispatch(eventType, worldEvent);
    }

    /// <summary>
    /// Starts queueing published events. Used by the world tick loop.
    /// </summary>
    public void BeginEventQueue()
    {
        using var trace = _traceLogger.BeginScope("EventBus.BeginEventQueue");

        if (_isQueueingEvents)
        {
            throw new InvalidOperationException("The event queue is already active.");
        }

        _isQueueingEvents = true;
    }

    /// <summary>
    /// Dispatches queued events in first-in, first-out order.
    /// Events published by handlers during this flush are appended and dispatched deterministically.
    /// </summary>
    public void FlushQueuedEvents()
    {
        using var trace = _traceLogger.BeginScope("EventBus.FlushQueuedEvents");

        while (_queuedEvents.Count > 0)
        {
            var queuedEvent = _queuedEvents.Dequeue();
            Dispatch(queuedEvent.EventType, queuedEvent.Payload);
        }
    }

    /// <summary>
    /// Clears queued events without dispatching them.
    /// </summary>
    public void ClearQueuedEvents()
    {
        using var trace = _traceLogger.BeginScope("EventBus.ClearQueuedEvents");
        _queuedEvents.Clear();
    }

    /// <summary>
    /// Stops queueing events. The queue must be flushed or cleared before this is called.
    /// </summary>
    public void EndEventQueue()
    {
        using var trace = _traceLogger.BeginScope("EventBus.EndEventQueue");

        if (_queuedEvents.Count > 0)
        {
            throw new InvalidOperationException("The event queue cannot end while events are still queued.");
        }

        _isQueueingEvents = false;
    }

    private void Dispatch(Type eventType, object worldEvent)
    {
        using var trace = _traceLogger.BeginScope($"EventBus.Dispatch({eventType.Name})");

        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            return;
        }

        foreach (var handler in handlers.ToArray())
        {
            using var handlerTrace = _traceLogger.BeginScope($"EventHandler.Invoke({eventType.Name})");
            handler.DynamicInvoke(worldEvent);
        }
    }

    /// <summary>
    /// Represents a disposable subscription to an event handler list.
    /// </summary>
    private sealed class Subscription : IDisposable
    {
        private readonly Action _unsubscribe;
        private readonly ITraceLogger _traceLogger;
        private bool _isDisposed;

        public Subscription(Action unsubscribe)
            : this(unsubscribe, NoOpTraceLogger.Instance)
        {
        }

        public Subscription(Action unsubscribe, ITraceLogger traceLogger)
        {
            _unsubscribe = unsubscribe;
            _traceLogger = traceLogger;
        }

        public void Dispose()
        {
            using var trace = _traceLogger.BeginScope("EventBus.Subscription.Dispose");

            if (_isDisposed)
            {
                return;
            }

            _unsubscribe();
            _isDisposed = true;
        }
    }

    private sealed record QueuedEvent(Type EventType, object Payload);
}
