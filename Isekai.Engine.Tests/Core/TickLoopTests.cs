using Isekai.Engine.Core;
using Isekai.Engine.Core.System;
using Isekai.Engine.Exceptions;
using Isekai.Engine.Tests.TestDoubles;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for deterministic tick execution.
/// </summary>
public sealed class TickLoopTests
{
    [Fact]
    public void Tick_ExecutesRegisteredSystem()
    {
        var world = new WorldState();
        var system = new CountingTestSystem();

        world.RegisterSystem(system);
        world.Tick(TimeSpan.FromMilliseconds(16));

        Assert.Equal(1, system.ExecutionCount);
        Assert.Equal(TimeSpan.FromMilliseconds(16), system.LastDeltaTime);
    }

    [Fact]
    public void Tick_ExecutesSystemsInRegistrationOrder()
    {
        var world = new WorldState();
        var executionOrder = new List<int>();

        world.RegisterSystem(new OrderedTestSystem(1, executionOrder));
        world.RegisterSystem(new OrderedTestSystem(2, executionOrder));
        world.RegisterSystem(new OrderedTestSystem(3, executionOrder));

        world.Tick(TimeSpan.FromSeconds(1));

        Assert.Equal(new[] { 1, 2, 3 }, executionOrder);
    }

    [Fact]
    public void Tick_AdvancesSimulationTimeAndReturnsResult()
    {
        var world = new WorldState();

        var result = world.Tick(TimeSpan.FromSeconds(3));

        Assert.Equal(TimeSpan.FromSeconds(3), world.Time.Elapsed);
        Assert.Equal(TimeSpan.FromSeconds(3), world.Time.Delta);
        Assert.Equal<ulong>(1, world.Time.TickCount);
        Assert.Equal(world.Time.Elapsed, result.ElapsedTime);
        Assert.Equal(world.Time.TickCount, result.TickCount);
    }

    [Fact]
    public void Tick_ThrowsWhenDeltaTimeIsNegative()
    {
        var world = new WorldState();

        Assert.Throws<InvalidTickDeltaException>(() => world.Tick(TimeSpan.FromTicks(-1)));
    }

    [Fact]
    public void Tick_DefersCreatedEntitiesUntilAllSystemsHaveExecuted()
    {
        var world = new WorldState();
        var creationSystem = new EntityCreationSystem();
        var observerSystem = new EntityCountObserverSystem();

        world.RegisterSystem(creationSystem);
        world.RegisterSystem(observerSystem);

        world.Tick(TimeSpan.FromSeconds(1));

        Assert.NotNull(creationSystem.CreatedEntity);
        Assert.False(creationSystem.WasCreatedEntityVisibleDuringTick);
        Assert.Equal(0, observerSystem.EntityCountDuringTick);
        Assert.True(world.ContainsEntity(creationSystem.CreatedEntity.Id));
        Assert.Single(world.Entities);
    }

    [Fact]
    public void Tick_DefersRemovedEntitiesUntilAllSystemsHaveExecuted()
    {
        var world = new WorldState();
        var entity = world.CreateEntity();
        var removalSystem = new EntityRemovalSystem(entity.Id);
        var observerSystem = new EntityCountObserverSystem();

        world.RegisterSystem(removalSystem);
        world.RegisterSystem(observerSystem);

        world.Tick(TimeSpan.FromSeconds(1));

        Assert.True(removalSystem.RemoveResult);
        Assert.True(removalSystem.WasEntityVisibleAfterRemoveRequest);
        Assert.Equal(1, observerSystem.EntityCountDuringTick);
        Assert.False(world.ContainsEntity(entity.Id));
        Assert.Empty(world.Entities);
    }

    [Fact]
    public void RegisterSystem_ThrowsWhenCalledDuringTick()
    {
        var world = new WorldState();
        world.RegisterSystem(new SystemRegistrationSystem());

        Assert.Throws<SystemRegistrationDuringTickException>(() => world.Tick(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void Tick_ThrowsWhenCalledWhileTickIsAlreadyRunning()
    {
        var world = new WorldState();
        world.RegisterSystem(new ReentrantTickSystem());

        Assert.Throws<ReentrantTickException>(() => world.Tick(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void Tick_DispatchesQueuedEventsAfterAllSystemsHaveExecuted()
    {
        var world = new WorldState();
        var executionOrder = new List<string>();

        world.EventBus.Subscribe<TestTickEvent>(_ => executionOrder.Add("event"));
        world.RegisterSystem(new EventPublishingSystem(executionOrder));
        world.RegisterSystem(new EventObserverSystem(executionOrder));

        world.Tick(TimeSpan.FromSeconds(1));

        Assert.Equal(new[] { "publisher-system", "observer-system", "event" }, executionOrder);
    }

    [Fact]
    public void Tick_DispatchesQueuedEventsBeforeDeferredWorldMutations()
    {
        var world = new WorldState();
        var entity = world.CreateEntity();
        var wasEntityVisibleDuringEvent = false;

        world.EventBus.Subscribe<TestTickEvent>(_ => wasEntityVisibleDuringEvent = world.ContainsEntity(entity.Id));
        world.RegisterSystem(new EntityRemovalSystem(entity.Id));
        world.RegisterSystem(new TickEventPublishingSystem());

        world.Tick(TimeSpan.FromSeconds(1));

        Assert.True(wasEntityVisibleDuringEvent);
        Assert.False(world.ContainsEntity(entity.Id));
    }

    /// <summary>
    /// Test system that appends its order marker when executed.
    /// </summary>
    private sealed class OrderedTestSystem : IWorldSystem
    {
        private readonly int _order;
        private readonly List<int> _executionOrder;

        public OrderedTestSystem(int order, List<int> executionOrder)
        {
            _order = order;
            _executionOrder = executionOrder;
        }

        public void Execute(WorldSystemExecutionContext context)
        {
            _executionOrder.Add(_order);
        }
    }

    /// <summary>
    /// Test system that creates an entity during a tick.
    /// </summary>
    private sealed class EntityCreationSystem : IWorldSystem
    {
        public Entity? CreatedEntity { get; private set; }

        public bool WasCreatedEntityVisibleDuringTick { get; private set; }

        public void Execute(WorldSystemExecutionContext context)
        {
            CreatedEntity = context.World.CreateEntity();
            WasCreatedEntityVisibleDuringTick = context.World.ContainsEntity(CreatedEntity.Id);
        }
    }

    /// <summary>
    /// Test system that records how many entities are visible during execution.
    /// </summary>
    private sealed class EntityCountObserverSystem : IWorldSystem
    {
        public int EntityCountDuringTick { get; private set; }

        public void Execute(WorldSystemExecutionContext context)
        {
            EntityCountDuringTick = context.World.Entities.Count;
        }
    }

    /// <summary>
    /// Test system that requests an entity removal during a tick.
    /// </summary>
    private sealed class EntityRemovalSystem : IWorldSystem
    {
        private readonly EntityId _entityId;

        public EntityRemovalSystem(EntityId entityId)
        {
            _entityId = entityId;
        }

        public bool RemoveResult { get; private set; }

        public bool WasEntityVisibleAfterRemoveRequest { get; private set; }

        public void Execute(WorldSystemExecutionContext context)
        {
            RemoveResult = context.World.RemoveEntity(_entityId);
            WasEntityVisibleAfterRemoveRequest = context.World.ContainsEntity(_entityId);
        }
    }

    /// <summary>
    /// Test system that tries to register another system during a tick.
    /// </summary>
    private sealed class SystemRegistrationSystem : IWorldSystem
    {
        public void Execute(WorldSystemExecutionContext context)
        {
            context.World.RegisterSystem(new OrderedTestSystem(1, new List<int>()));
        }
    }

    /// <summary>
    /// Test system that tries to start another tick during a tick.
    /// </summary>
    private sealed class ReentrantTickSystem : IWorldSystem
    {
        public void Execute(WorldSystemExecutionContext context)
        {
            context.World.Tick(TimeSpan.FromSeconds(1));
        }
    }

    /// <summary>
    /// Test system that publishes an event during a tick.
    /// </summary>
    private sealed class EventPublishingSystem : IWorldSystem
    {
        private readonly List<string> _executionOrder;

        public EventPublishingSystem(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public void Execute(WorldSystemExecutionContext context)
        {
            _executionOrder.Add("publisher-system");
            context.EventBus.Publish(new TestTickEvent());
        }
    }

    /// <summary>
    /// Test system that records execution after another system published an event.
    /// </summary>
    private sealed class EventObserverSystem : IWorldSystem
    {
        private readonly List<string> _executionOrder;

        public EventObserverSystem(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public void Execute(WorldSystemExecutionContext context)
        {
            _executionOrder.Add("observer-system");
        }
    }

    /// <summary>
    /// Test system that publishes a neutral tick event.
    /// </summary>
    private sealed class TickEventPublishingSystem : IWorldSystem
    {
        public void Execute(WorldSystemExecutionContext context)
        {
            context.EventBus.Publish(new TestTickEvent());
        }
    }

    /// <summary>
    /// Event type used by tick loop tests.
    /// </summary>
    private sealed record TestTickEvent;
}
