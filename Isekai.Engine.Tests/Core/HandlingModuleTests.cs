using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Modules.Body;
using Isekai.Engine.Modules.BodyCapabilities;
using Isekai.Engine.Modules.Handling;
using Isekai.Engine.Modules.Impact;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for generic held entity handling behavior.
/// </summary>
public sealed class HandlingModuleTests
{
    [Fact]
    public void HandledImpactSystem_CreatesImpactRequestOnHeldEntity()
    {
        var world = CreateWorld();
        var actor = world.CreateEntity();
        var held = world.CreateEntity();
        var contact = world.CreateEntity();
        var target = world.CreateEntity();
        actor.AddComponent(new GripCapabilityComponent(
            MaxGripForce: 100,
            ManipulationForce: 50,
            Precision: 0.8));
        actor.AddComponent(new HeldEntitiesComponent(new[]
        {
            new HeldEntity(held.Id, "right_hand", GripQuality: 0.5)
        }));
        actor.AddComponent(new HandledImpactComponent(
            held.Id,
            contact.Id,
            target.Id,
            Effort: 1,
            TicksRemaining: 3,
            TicksBetweenImpacts: 2,
            TargetBodyPartId: "trunk"));

        world.RegisterSystem(new HandledImpactSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var request = held.GetComponent<ImpactRequestComponent>();

        Assert.Equal(contact.Id, request.ContactEntityId);
        Assert.Equal(target.Id, request.TargetEntityId);
        Assert.Equal("trunk", request.TargetBodyPartId);
        Assert.Equal(20, request.Force);
    }

    [Fact]
    public void HandledImpactSystem_RespectsTicksBetweenImpacts()
    {
        var world = CreateWorld();
        var actor = world.CreateEntity();
        var held = world.CreateEntity();
        var contact = world.CreateEntity();
        var target = world.CreateEntity();
        actor.AddComponent(new GripCapabilityComponent(100, 50, 1));
        actor.AddComponent(new HeldEntitiesComponent(new[]
        {
            new HeldEntity(held.Id, "right_hand", GripQuality: 1)
        }));
        actor.AddComponent(new HandledImpactComponent(
            held.Id,
            contact.Id,
            target.Id,
            Effort: 1,
            TicksRemaining: 3,
            TicksBetweenImpacts: 3));

        world.RegisterSystem(new HandledImpactSystem());

        world.Tick(TimeSpan.FromSeconds(1));
        Assert.True(held.HasComponent<ImpactRequestComponent>());

        world.Tick(TimeSpan.FromSeconds(1));
        Assert.False(held.HasComponent<ImpactRequestComponent>());

        world.Tick(TimeSpan.FromSeconds(1));
        Assert.False(held.HasComponent<ImpactRequestComponent>());
    }

    [Fact]
    public void HandledImpactSystem_DoesNotCreateRequestWhenEntityIsNotHeld()
    {
        var world = CreateWorld();
        var actor = world.CreateEntity();
        var held = world.CreateEntity();
        var contact = world.CreateEntity();
        var target = world.CreateEntity();
        actor.AddComponent(new GripCapabilityComponent(100, 50, 1));
        actor.AddComponent(new HeldEntitiesComponent(Array.Empty<HeldEntity>()));
        actor.AddComponent(new HandledImpactComponent(
            held.Id,
            contact.Id,
            target.Id,
            Effort: 1,
            TicksRemaining: 3,
            TicksBetweenImpacts: 1));

        world.RegisterSystem(new HandledImpactSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.False(held.HasComponent<ImpactRequestComponent>());
    }

    [Fact]
    public void BodyImpactSystem_CreatesImpactRequestFromAuthorizedBodyPart()
    {
        var world = CreateWorld();
        var actor = world.CreateEntity();
        var target = world.CreateEntity();
        actor.AddComponent(new BodyImpactCapabilityComponent(
            MaxForce: 120,
            ImpactForce: 80,
            Precision: 0.75));
        actor.AddComponent(new BodyContactSurfacesComponent(new[]
        {
            new BodyContactSurface(
                "front_hoof",
                "front_legs",
                Hardness: 1.1,
                Sharpness: 0.3,
                Penetration: 0.8,
                EdgeRetention: 1.2,
                ContactArea: 0.02)
        }));
        actor.AddComponent(new BodyImpactComponent(
            "front_hoof",
            target.Id,
            Effort: 1,
            TicksRemaining: 3,
            TicksBetweenImpacts: 2,
            TargetBodyPartId: "torso"));

        world.RegisterSystem(new BodyImpactSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var request = actor.GetComponent<ImpactRequestComponent>();
        var surface = actor.GetComponent<ContactSurfaceComponent>();

        Assert.Equal(actor.Id, request.ContactEntityId);
        Assert.Equal(target.Id, request.TargetEntityId);
        Assert.Equal("torso", request.TargetBodyPartId);
        Assert.Equal(60, request.Force);
        Assert.Equal(1.1, surface.Hardness);
    }

    [Fact]
    public void BodyImpactSystem_DoesNotCreateRequestForUnauthorizedRole()
    {
        var world = CreateWorld();
        var actor = world.CreateEntity();
        var target = world.CreateEntity();
        actor.AddComponent(new BodyImpactCapabilityComponent(120, 80, 1));
        actor.AddComponent(new BodyContactSurfacesComponent(new[]
        {
            new BodyContactSurface("front_hoof", "front_legs", 1.1, 0.3, 0.8, 1.2, 0.02)
        }));
        actor.AddComponent(new BodyImpactComponent(
            "neck_strike",
            target.Id,
            Effort: 1,
            TicksRemaining: 3,
            TicksBetweenImpacts: 1));

        world.RegisterSystem(new BodyImpactSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.False(actor.HasComponent<ImpactRequestComponent>());
    }

    [Fact]
    public void BodyImpactSystem_DoesNotCreateRequestWhenBodyPartIsDestroyed()
    {
        var world = CreateWorld();
        var actor = world.CreateEntity();
        var target = world.CreateEntity();
        actor.AddComponent(new BodyStateComponent(
            DefinitionReference<BodyDefinition>.From("body.test"),
            new[]
            {
                new BodyPartState("front_legs", Integrity: 0, MaxIntegrity: 70)
            }));
        actor.AddComponent(new BodyImpactCapabilityComponent(120, 80, 1));
        actor.AddComponent(new BodyContactSurfacesComponent(new[]
        {
            new BodyContactSurface("front_hoof", "front_legs", 1.1, 0.3, 0.8, 1.2, 0.02)
        }));
        actor.AddComponent(new BodyImpactComponent(
            "front_hoof",
            target.Id,
            Effort: 1,
            TicksRemaining: 3,
            TicksBetweenImpacts: 1));

        world.RegisterSystem(new BodyImpactSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.False(actor.HasComponent<ImpactRequestComponent>());
    }

    [Fact]
    public void HandlingModule_RegistersHandledImpactSystemOnly()
    {
        var world = CreateWorld();
        var module = new HandlingModule();

        module.RegisterSystems(world);

        Assert.Contains(world.Systems, system => system is HandledImpactSystem);
        Assert.DoesNotContain(world.Systems, system => system is BodyImpactSystem);
    }

    private static WorldState CreateWorld()
    {
        var registry = new DefinitionRegistry();
        registry.Freeze();

        return new WorldState(
            new SimulationTime(),
            new EventBus(),
            registry,
            NoOpTraceLogger.Instance);
    }
}
