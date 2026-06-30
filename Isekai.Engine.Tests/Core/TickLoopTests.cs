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
}
