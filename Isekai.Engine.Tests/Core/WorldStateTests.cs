using Isekai.Engine.Core;
using Isekai.Engine.Exceptions;
using Isekai.Engine.Tests.TestDoubles;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for world state ownership and consistency.
/// </summary>
public sealed class WorldStateTests
{
    [Fact]
    public void CreateEntity_RegistersEntityInWorld()
    {
        var world = new WorldState();

        var entity = world.CreateEntity();

        Assert.True(world.ContainsEntity(entity.Id));
        Assert.Same(entity, world.GetEntity(entity.Id));
        Assert.Single(world.Entities);
    }

    [Fact]
    public void RemoveEntity_RemovesEntityFromWorld()
    {
        var world = new WorldState();
        var entity = world.CreateEntity();

        var removed = world.RemoveEntity(entity.Id);

        Assert.True(removed);
        Assert.False(world.ContainsEntity(entity.Id));
        Assert.Throws<EntityNotFoundException>(() => world.GetEntity(entity.Id));
    }

    [Fact]
    public void WorldState_RemainsCoherentWhenEntityHasMultipleComponents()
    {
        var world = new WorldState();
        var entity = world.CreateEntity();

        entity.AddComponent(new TestComponent(10));
        entity.AddComponent(new SecondTestComponent("neutral"));

        Assert.Single(world.Entities);
        Assert.Equal(2, world.GetEntity(entity.Id).Components.Count);
        Assert.True(world.GetEntity(entity.Id).HasComponent<TestComponent>());
        Assert.True(world.GetEntity(entity.Id).HasComponent<SecondTestComponent>());
    }

    [Fact]
    public void EntitiesWith_ReturnsEntitiesThatOwnRequestedComponent()
    {
        var world = new WorldState();
        var matchingEntity = world.CreateEntity();
        var ignoredEntity = world.CreateEntity();

        matchingEntity.AddComponent(new TestComponent(10));
        ignoredEntity.AddComponent(new SecondTestComponent("neutral"));

        var result = world.EntitiesWith<TestComponent>();

        Assert.Single(result);
        Assert.Same(matchingEntity, result.Single());
    }

    [Fact]
    public void EntitiesWithTwoComponents_ReturnsEntitiesThatOwnBothComponents()
    {
        var world = new WorldState();
        var matchingEntity = world.CreateEntity();
        var ignoredEntity = world.CreateEntity();

        matchingEntity.AddComponent(new TestComponent(10));
        matchingEntity.AddComponent(new SecondTestComponent("neutral"));
        ignoredEntity.AddComponent(new TestComponent(20));

        var result = world.EntitiesWith<TestComponent, SecondTestComponent>();

        Assert.Single(result);
        Assert.Same(matchingEntity, result.Single());
    }
}
