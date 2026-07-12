using Isekai.Engine.Core;
using Isekai.Engine.Exceptions;
using Isekai.Engine.Tests.TestDoubles;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for entity component storage.
/// </summary>
public sealed class EntityTests
{
    [Fact]
    public void Entity_CanBeCreated()
    {
        var entity = new Entity(EntityId.New());

        Assert.NotEqual(default, entity.Id);
        Assert.Empty(entity.Components);
    }

    [Fact]
    public void AddComponent_AddsDataComponent()
    {
        var entity = new Entity(EntityId.New());
        var component = new TestComponent(42);

        entity.AddComponent(component);

        Assert.True(entity.HasComponent<TestComponent>());
        Assert.Same(component, entity.GetComponent<TestComponent>());
        Assert.Single(entity.Components);
    }

    [Fact]
    public void AddComponent_ThrowsWhenComponentTypeAlreadyExists()
    {
        var entity = new Entity(EntityId.New());
        entity.AddComponent(new TestComponent(1));

        Assert.Throws<ComponentAlreadyExistsException>(() => entity.AddComponent(new TestComponent(2)));
    }

    [Fact]
    public void SetComponent_AddsOrReplacesDataComponent()
    {
        var entity = new Entity(EntityId.New());

        entity.SetComponent(new TestComponent(1));
        entity.SetComponent(new TestComponent(2));

        Assert.Single(entity.Components);
        Assert.Equal(2, entity.GetComponent<TestComponent>().Value);
    }
}
