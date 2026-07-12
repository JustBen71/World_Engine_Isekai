using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Modules.Body;
using Isekai.Engine.Modules.Composition;
using Isekai.Engine.Modules.Materials;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for generic physical composition behavior.
/// </summary>
public sealed class CompositionModuleTests
{
    [Fact]
    public void CompositeComponent_CanReferenceAnyNumberOfParts()
    {
        var world = CreateWorld();
        var partA = world.CreateEntity();
        var partB = world.CreateEntity();
        var composite = world.CreateEntity();

        composite.AddComponent(new CompositeComponent(new[]
        {
            new CompositePart("handle", partA.Id, IsStructural: true),
            new CompositePart("head", partB.Id, IsStructural: true)
        }));

        var component = composite.GetComponent<CompositeComponent>();

        Assert.Equal(2, component.Parts.Count);
    }

    [Fact]
    public void CompositeMembershipSystem_AddsMembershipToParts()
    {
        var world = CreateWorld();
        var part = world.CreateEntity();
        var composite = world.CreateEntity();
        composite.AddComponent(new CompositeComponent(new[]
        {
            new CompositePart("core", part.Id, IsStructural: true)
        }));

        world.RegisterSystem(new CompositeMembershipSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var membership = part.GetComponent<CompositeMembershipComponent>();

        Assert.Equal(composite.Id, membership.CompositeEntityId);
        Assert.Equal("core", membership.Role);
        Assert.True(membership.IsStructural);
    }

    [Fact]
    public void CompositeIntegritySystem_MarksCompositeCompleteWhenStructuralPartsExist()
    {
        var world = CreateWorld();
        var part = world.CreateEntity();
        var composite = world.CreateEntity();
        composite.AddComponent(new CompositeComponent(new[]
        {
            new CompositePart("core", part.Id, IsStructural: true)
        }));

        world.RegisterSystem(new CompositeIntegritySystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var integrity = composite.GetComponent<CompositeIntegrityComponent>();

        Assert.True(integrity.IsStructurallyComplete);
        Assert.Equal(1, integrity.NormalizedIntegrity);
    }

    [Fact]
    public void CompositeIntegritySystem_MarksCompositeIncompleteWhenStructuralPartIsMissing()
    {
        var world = CreateWorld();
        var part = world.CreateEntity();
        var missingPartId = part.Id;
        world.RemoveEntity(part.Id);
        var composite = world.CreateEntity();
        composite.AddComponent(new CompositeComponent(new[]
        {
            new CompositePart("core", missingPartId, IsStructural: true)
        }));

        world.RegisterSystem(new CompositeIntegritySystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var integrity = composite.GetComponent<CompositeIntegrityComponent>();

        Assert.False(integrity.IsStructurallyComplete);
        Assert.Equal(0, integrity.NormalizedIntegrity);
        Assert.Contains("core", integrity.MissingStructuralRoles);
    }

    [Fact]
    public void CompositeIntegritySystem_UsesPartBodyIntegrityWhenAvailable()
    {
        var world = CreateWorld();
        var part = world.CreateEntity();
        part.AddComponent(new BodyIntegrityComponent(0.25));
        var composite = world.CreateEntity();
        composite.AddComponent(new CompositeComponent(new[]
        {
            new CompositePart("damaged_board", part.Id, IsStructural: true)
        }));

        world.RegisterSystem(new CompositeIntegritySystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var integrity = composite.GetComponent<CompositeIntegrityComponent>();

        Assert.Equal(0.25, integrity.NormalizedIntegrity);
        Assert.Equal(part.Id, integrity.WeakestStructuralPartId);
        Assert.Equal("damaged_board", integrity.WeakestStructuralRole);
    }

    [Fact]
    public void CompositeMassSystem_SumsOwnIntegratedMassAndPartMasses()
    {
        var world = CreateWorld();
        var part = world.CreateEntity();
        part.AddComponent(new MaterialMassComponent(2));
        var composite = world.CreateEntity();
        composite.AddComponent(new MaterialMassComponent(0.5));
        composite.AddComponent(new CompositeComponent(new[]
        {
            new CompositePart("core", part.Id, IsStructural: true)
        }));

        world.RegisterSystem(new CompositeMassSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var mass = composite.GetComponent<CompositeMassComponent>();

        Assert.Equal(2.5, mass.Kilograms);
    }

    [Fact]
    public void CompositionModule_RegistersCompositionSystems()
    {
        var world = CreateWorld();
        var module = new CompositionModule();

        module.RegisterSystems(world);

        Assert.Contains(world.Systems, system => system is CompositeMembershipSystem);
        Assert.Contains(world.Systems, system => system is CompositeIntegritySystem);
        Assert.Contains(world.Systems, system => system is CompositeMassSystem);
    }

    private static WorldState CreateWorld()
    {
        var registry = new DefinitionRegistry();
        registry.Register(new MaterialDefinition(
            DefinitionId.From("material.test"),
            Density: 1,
            SpecificHeatCapacity: 1,
            ThermalConductivity: 1));
        registry.Freeze();

        return new WorldState(
            new SimulationTime(),
            new EventBus(),
            registry,
            NoOpTraceLogger.Instance);
    }
}
