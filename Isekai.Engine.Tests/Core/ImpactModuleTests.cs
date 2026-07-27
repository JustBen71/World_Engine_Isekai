using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Modules.Body;
using Isekai.Engine.Modules.Composition;
using Isekai.Engine.Modules.Impact;
using Isekai.Engine.Modules.Injuries;
using Isekai.Engine.Modules.Materials;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for generic contact and impact behavior.
/// </summary>
public sealed class ImpactModuleTests
{
    [Fact]
    public void ImpactResolutionSystem_StoresImpactResultOnSource()
    {
        var world = CreateWorld();
        var contact = world.CreateEntity();
        contact.AddComponent(new ContactSurfaceComponent(
            Hardness: 2.0,
            Sharpness: 2.4,
            Penetration: 1.8,
            EdgeRetention: 2.5,
            ContactArea: 0.015));
        var target = world.CreateEntity();
        target.AddComponent(new ImpactResistanceComponent(
            Hardness: 1.0,
            Toughness: 1.0,
            FractureResistance: 1.0));
        var source = world.CreateEntity();
        source.AddComponent(new ImpactRequestComponent(
            contact.Id,
            target.Id,
            Force: 80,
            TargetBodyPartId: "trunk"));

        world.RegisterSystem(new ImpactResolutionSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var result = source.GetComponent<ImpactResultComponent>();

        Assert.Equal(contact.Id, result.ContactEntityId);
        Assert.Equal(target.Id, result.TargetEntityId);
        Assert.Equal("trunk", result.TargetBodyPartId);
        Assert.True(result.ImpactRatio > 0);
        Assert.NotEqual(ImpactOutcome.None, result.Outcome);
    }

    [Fact]
    public void ImpactResolutionSystem_DifferentiatesContactSurfaces()
    {
        var world = CreateWorld();
        var weakContact = world.CreateEntity();
        weakContact.AddComponent(new ContactSurfaceComponent(
            Hardness: 1.0,
            Sharpness: 0.4,
            Penetration: 0.5,
            EdgeRetention: 0.8,
            ContactArea: 0.03));
        var strongContact = world.CreateEntity();
        strongContact.AddComponent(new ContactSurfaceComponent(
            Hardness: 2.0,
            Sharpness: 2.4,
            Penetration: 1.8,
            EdgeRetention: 2.5,
            ContactArea: 0.015));
        var target = world.CreateEntity();
        target.AddComponent(new ImpactResistanceComponent(
            Hardness: 1.0,
            Toughness: 1.0,
            FractureResistance: 1.0));
        var weakSource = world.CreateEntity();
        weakSource.AddComponent(new ImpactRequestComponent(weakContact.Id, target.Id, Force: 80));
        var strongSource = world.CreateEntity();
        strongSource.AddComponent(new ImpactRequestComponent(strongContact.Id, target.Id, Force: 80));

        world.RegisterSystem(new ImpactResolutionSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var weakResult = weakSource.GetComponent<ImpactResultComponent>();
        var strongResult = strongSource.GetComponent<ImpactResultComponent>();

        Assert.True(strongResult.ImpactRatio > weakResult.ImpactRatio);
    }

    [Fact]
    public void ImpactResolutionSystem_UsesTargetResistance()
    {
        var world = CreateWorld();
        var contact = world.CreateEntity();
        contact.AddComponent(new ContactSurfaceComponent(
            Hardness: 2.0,
            Sharpness: 2.4,
            Penetration: 1.8,
            EdgeRetention: 2.5,
            ContactArea: 0.015));
        var weakTarget = world.CreateEntity();
        weakTarget.AddComponent(new ImpactResistanceComponent(1.0, 1.0, 1.0));
        var strongTarget = world.CreateEntity();
        strongTarget.AddComponent(new ImpactResistanceComponent(2.0, 2.0, 2.0));
        var weakTargetSource = world.CreateEntity();
        weakTargetSource.AddComponent(new ImpactRequestComponent(contact.Id, weakTarget.Id, Force: 80));
        var strongTargetSource = world.CreateEntity();
        strongTargetSource.AddComponent(new ImpactRequestComponent(contact.Id, strongTarget.Id, Force: 80));

        world.RegisterSystem(new ImpactResolutionSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var weakTargetResult = weakTargetSource.GetComponent<ImpactResultComponent>();
        var strongTargetResult = strongTargetSource.GetComponent<ImpactResultComponent>();

        Assert.True(weakTargetResult.ImpactRatio > strongTargetResult.ImpactRatio);
    }

    [Fact]
    public void ImpactResolutionSystem_UsesSourceMassWithoutRequiringGameplayKnowledge()
    {
        var world = CreateWorld();
        var contact = world.CreateEntity();
        contact.AddComponent(new ContactSurfaceComponent(
            Hardness: 1.5,
            Sharpness: 1.2,
            Penetration: 1.1,
            EdgeRetention: 1.0,
            ContactArea: 0.01));
        var target = world.CreateEntity();
        target.AddComponent(new ImpactResistanceComponent(1.0, 1.0, 1.0));
        var lightSource = world.CreateEntity();
        lightSource.AddComponent(new CompositeMassComponent(0.25));
        lightSource.AddComponent(new ImpactRequestComponent(contact.Id, target.Id, Force: 60));
        var heavySource = world.CreateEntity();
        heavySource.AddComponent(new CompositeMassComponent(4.0));
        heavySource.AddComponent(new ImpactRequestComponent(contact.Id, target.Id, Force: 60));

        world.RegisterSystem(new ImpactResolutionSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var lightResult = lightSource.GetComponent<ImpactResultComponent>();
        var heavyResult = heavySource.GetComponent<ImpactResultComponent>();

        Assert.True(heavyResult.ImpactRatio > lightResult.ImpactRatio);
    }

    [Fact]
    public void ImpactResolutionSystem_DoesNotLowerEfficiencyFromPartialContactWear()
    {
        var world = CreateWorld();
        var freshContact = CreateContactBody(world, integrity: 35);
        freshContact.AddComponent(new ContactSurfaceComponent(1.2, 1.8, 1.4, 0.5, 0.012));
        var wornContact = CreateContactBody(world, integrity: 4);
        wornContact.AddComponent(new ContactSurfaceComponent(1.2, 1.8, 1.4, 0.5, 0.012));
        var target = world.CreateEntity();
        target.AddComponent(new ImpactResistanceComponent(0.8, 1.2, 0.9));
        var freshSource = world.CreateEntity();
        freshSource.AddComponent(new ImpactRequestComponent(freshContact.Id, target.Id, Force: 45));
        var wornSource = world.CreateEntity();
        wornSource.AddComponent(new ImpactRequestComponent(wornContact.Id, target.Id, Force: 45));

        world.RegisterSystem(new ImpactResolutionSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var freshResult = freshSource.GetComponent<ImpactResultComponent>();
        var wornResult = wornSource.GetComponent<ImpactResultComponent>();

        Assert.Equal(freshResult.ImpactRatio, wornResult.ImpactRatio, precision: 10);
        Assert.Equal(freshResult.Outcome, wornResult.Outcome);
    }

    [Fact]
    public void ImpactResolutionSystem_IgnoresDestroyedContactSurface()
    {
        var world = CreateWorld();
        var destroyedContact = CreateContactBody(world, integrity: 0);
        destroyedContact.AddComponent(new ContactSurfaceComponent(1.2, 1.8, 1.4, 0.5, 0.012));
        var target = world.CreateEntity();
        target.AddComponent(new ImpactResistanceComponent(0.8, 1.2, 0.9));
        var source = world.CreateEntity();
        source.AddComponent(new ImpactRequestComponent(destroyedContact.Id, target.Id, Force: 45));

        world.RegisterSystem(new ImpactResolutionSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.False(source.HasComponent<ImpactResultComponent>());
    }

    [Fact]
    public void ImpactResolutionSystem_PublishesImpactResolvedEvent()
    {
        var world = CreateWorld();
        var contact = world.CreateEntity();
        contact.AddComponent(new ContactSurfaceComponent(1, 1, 1, 1, 0.01));
        var target = world.CreateEntity();
        var source = world.CreateEntity();
        source.AddComponent(new ImpactRequestComponent(contact.Id, target.Id, Force: 80));
        ImpactResolvedEvent? captured = null;
        world.EventBus.Subscribe<ImpactResolvedEvent>(worldEvent => captured = worldEvent);

        world.RegisterSystem(new ImpactResolutionSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.NotNull(captured);
        Assert.Equal(source.Id, captured.SourceEntityId);
    }

    [Fact]
    public void ImpactModule_RegistersImpactResolutionSystem()
    {
        var world = CreateWorld();
        var module = new ImpactModule();

        module.RegisterSystems(world);

        Assert.Contains(world.Systems, system => system is ImpactResolutionSystem);
        Assert.Contains(world.Systems, system => system is ImpactToBodyDamageSystem);
        Assert.Contains(world.Systems, system => system is ImpactContactWearSystem);
    }

    [Fact]
    public void ImpactToBodyDamageSystem_ReducesTargetBodyPartIntegrity()
    {
        var world = CreateWorld();
        var contact = world.CreateEntity();
        contact.AddComponent(new ContactSurfaceComponent(1.2, 1.8, 1.4, 0.5, 0.012));
        var target = CreateBodyTarget(world);
        target.AddComponent(new ImpactResistanceComponent(0.8, 1.2, 0.9));
        var source = world.CreateEntity();
        source.AddComponent(new ImpactRequestComponent(
            contact.Id,
            target.Id,
            Force: 45,
            TargetBodyPartId: "trunk"));

        world.RegisterSystem(new ImpactResolutionSystem());
        world.RegisterSystem(new ImpactToBodyDamageSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var body = target.GetComponent<BodyStateComponent>();
        var trunk = body.Parts.Single(part => part.PartId == "trunk");

        Assert.True(trunk.Integrity < 120);
        Assert.True(target.GetComponent<BodyIntegrityComponent>().NormalizedIntegrity < 1);
    }

    [Fact]
    public void ImpactToBodyDamageSystem_IgnoresStaleResultWhenRequestIsRemoved()
    {
        var world = CreateWorld();
        var target = CreateBodyTarget(world);
        var source = world.CreateEntity();
        source.AddComponent(new ImpactResultComponent(
            ContactEntityId: EntityId.New(),
            TargetEntityId: target.Id,
            TargetBodyPartId: "trunk",
            Force: 100,
            ImpactRatio: 5,
            Outcome: ImpactOutcome.Fracture));

        world.RegisterSystem(new ImpactToBodyDamageSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var trunk = target.GetComponent<BodyStateComponent>()
            .Parts
            .Single(part => part.PartId == "trunk");

        Assert.Equal(120, trunk.Integrity);
    }

    [Fact]
    public void ImpactContactWearSystem_ReducesContactEntityIntegrity()
    {
        var world = CreateWorld();
        var contact = CreateContactBody(world);
        contact.AddComponent(new ContactSurfaceComponent(1.2, 1.8, 1.4, 0.5, 0.012));
        var target = CreateBodyTarget(world);
        target.AddComponent(new ImpactResistanceComponent(0.8, 1.2, 0.9));
        var source = world.CreateEntity();
        source.AddComponent(new ImpactRequestComponent(
            contact.Id,
            target.Id,
            Force: 45,
            TargetBodyPartId: "trunk"));

        world.RegisterSystem(new ImpactResolutionSystem());
        world.RegisterSystem(new ImpactContactWearSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var edge = contact.GetComponent<BodyStateComponent>()
            .Parts
            .Single(part => part.PartId == "edge");

        Assert.True(edge.Integrity < 35);
        Assert.True(contact.GetComponent<BodyIntegrityComponent>().NormalizedIntegrity < 1);
    }

    [Fact]
    public void ImpactInjuryBridgeSystem_AddsConfiguredInjuryToTarget()
    {
        var world = CreateWorld();
        var target = CreateBodyTarget(world);
        target.AddComponent(new ImpactInjuryProfileComponent(new[]
        {
            new ImpactInjuryRule(
                ImpactOutcome.Cut,
                DefinitionReference<InjuryDefinition>.From("injury.cut"),
                MinimumSeverity: 0.2,
                SeverityPerImpactRatio: 0.1,
                BleedingSeverity.Moderate)
        }));
        var source = world.CreateEntity();
        source.AddComponent(new ImpactRequestComponent(EntityId.New(), target.Id, Force: 45, TargetBodyPartId: "trunk"));
        source.AddComponent(new ImpactResultComponent(
            EntityId.New(),
            target.Id,
            "trunk",
            Force: 45,
            ImpactRatio: 2,
            ImpactOutcome.Cut));

        world.RegisterSystem(new ImpactInjuryBridgeSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var injury = target.GetComponent<InjuryComponent>().Injuries.Single();

        Assert.Equal("injury.cut", injury.Injury.Id.Value);
        Assert.Equal("trunk", injury.BodyPartId);
        Assert.Equal(0.4, injury.Severity, precision: 10);
        Assert.Equal(BleedingSeverity.Moderate, injury.BleedingSeverity);
    }

    private static WorldState CreateWorld()
    {
        var registry = new DefinitionRegistry();
        registry.Register(new MaterialDefinition(
            DefinitionId.From("material.test"),
            Density: 1,
            SpecificHeatCapacity: 1,
            ThermalConductivity: 1));
        registry.Register(new BodyDefinition(
            DefinitionId.From("body.tree"),
            new[]
            {
                new BodyPartDefinition(
                    "trunk",
                    "Trunk",
                    DefinitionReference<MaterialDefinition>.From("material.test"),
                    MaxIntegrity: 120),
                new BodyPartDefinition(
                    "branches",
                    "Branches",
                    DefinitionReference<MaterialDefinition>.From("material.test"),
                    MaxIntegrity: 60)
            }));
        registry.Register(new BodyDefinition(
            DefinitionId.From("body.contact"),
            new[]
            {
                new BodyPartDefinition(
                    "edge",
                    "Edge",
                    DefinitionReference<MaterialDefinition>.From("material.test"),
                    MaxIntegrity: 35)
            }));
        registry.Register(new InjuryDefinition(
            DefinitionId.From("injury.cut"),
            "Cut",
            IntegrityLossPerSeverityPerSecond: 0.2,
            BleedingSeverity.Moderate));
        registry.Freeze();

        return new WorldState(
            new SimulationTime(),
            new EventBus(),
            registry,
            NoOpTraceLogger.Instance);
    }

    private static Entity CreateBodyTarget(WorldState world)
    {
        var entity = world.CreateEntity();
        entity.AddComponent(new BodyStateComponent(
            DefinitionReference<BodyDefinition>.From("body.tree"),
            new[]
            {
                new BodyPartState("trunk", Integrity: 120, MaxIntegrity: 120),
                new BodyPartState("branches", Integrity: 60, MaxIntegrity: 60)
            }));
        entity.AddComponent(new BodyIntegrityComponent(1));

        return entity;
    }

    private static Entity CreateContactBody(WorldState world, double integrity = 35)
    {
        var entity = world.CreateEntity();
        entity.AddComponent(new BodyStateComponent(
            DefinitionReference<BodyDefinition>.From("body.contact"),
            new[]
            {
                new BodyPartState("edge", integrity, MaxIntegrity: 35)
            }));
        entity.AddComponent(new BodyIntegrityComponent(Math.Clamp(integrity / 35, 0, 1)));

        return entity;
    }
}
