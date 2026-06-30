using Isekai.Engine.Core;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for event bus publication and subscription.
/// </summary>
public sealed class EventBusTests
{
    [Fact]
    public void Publish_NotifiesSubscribedHandlers()
    {
        var eventBus = new EventBus();
        var received = 0;

        eventBus.Subscribe<TestEvent>(worldEvent => received = worldEvent.Value);
        eventBus.Publish(new TestEvent(7));

        Assert.Equal(7, received);
    }

    [Fact]
    public void DisposedSubscription_DoesNotReceiveEvents()
    {
        var eventBus = new EventBus();
        var receivedCount = 0;

        using (eventBus.Subscribe<TestEvent>(_ => receivedCount++))
        {
            eventBus.Publish(new TestEvent(1));
        }

        eventBus.Publish(new TestEvent(2));

        Assert.Equal(1, receivedCount);
    }

    [Fact]
    public void Publish_QueuesEventsWhenEventQueueIsActive()
    {
        var eventBus = new EventBus();
        var received = new List<int>();

        eventBus.Subscribe<TestEvent>(worldEvent => received.Add(worldEvent.Value));
        eventBus.BeginEventQueue();

        eventBus.Publish(new TestEvent(1));
        eventBus.Publish(new TestEvent(2));

        Assert.Empty(received);
        Assert.Equal(2, eventBus.QueuedEventCount);

        eventBus.FlushQueuedEvents();
        eventBus.EndEventQueue();

        Assert.Equal(new[] { 1, 2 }, received);
        Assert.Equal(0, eventBus.QueuedEventCount);
    }

    [Fact]
    public void FlushQueuedEvents_DispatchesEventsPublishedByHandlersInOrder()
    {
        var eventBus = new EventBus();
        var received = new List<int>();

        eventBus.Subscribe<TestEvent>(worldEvent =>
        {
            received.Add(worldEvent.Value);

            if (worldEvent.Value == 1)
            {
                eventBus.Publish(new TestEvent(2));
            }
        });

        eventBus.BeginEventQueue();
        eventBus.Publish(new TestEvent(1));

        eventBus.FlushQueuedEvents();
        eventBus.EndEventQueue();

        Assert.Equal(new[] { 1, 2 }, received);
    }

    /// <summary>
    /// Event type used by event bus tests.
    /// </summary>
    private sealed record TestEvent(int Value);
}
