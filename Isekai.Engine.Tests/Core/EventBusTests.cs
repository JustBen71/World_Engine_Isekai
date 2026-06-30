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

    /// <summary>
    /// Event type used by event bus tests.
    /// </summary>
    private sealed record TestEvent(int Value);
}
