using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Data.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Exceptions;
using Isekai.Engine.Modules.Body;
using Isekai.Engine.Modules.Healing;
using Isekai.Engine.Modules.Injuries;
using Isekai.Engine.Modules.Materials;
using Isekai.Engine.Modules.Vitals;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for the universal injury module.
/// </summary>
public sealed class InjuryModuleTests
{
    [Fact]
    public void RegisterInjuryDefinitions_RegistersInjuryDefinitionType()
    {
        var types = new DefinitionTypeRegistry();

        types.RegisterInjuryDefinitions();

        Assert.Contains("injury.definition", types.TypeNames);
    }

    [Fact]
    public void JsonLoader_LoadsInjuryDefinition()
    {
        var types = new DefinitionTypeRegistry();
        types.RegisterInjuryDefinitions();
        var loader = new JsonDefinitionLoader(types);

        const string json = """
        {
          "type": "injury.definition",
          "id": "injury.test",
          "name": "Test injury",
          "integrityLossPerSeverityPerSecond": 2,
          "defaultBleedingSeverity": "Moderate"
        }
        """;

        var registry = loader.LoadJson(json);
        var injury = registry.Get<InjuryDefinition>(DefinitionId.From("injury.test"));

        Assert.Equal(2, injury.IntegrityLossPerSeverityPerSecond);
        Assert.Equal("Test injury", injury.Name);
        Assert.Equal(BleedingSeverity.Moderate, injury.DefaultBleedingSeverity);
    }

    [Fact]
    public void InjuryDefinitionValidator_RejectsNegativeValues()
    {
        var registry = new DefinitionRegistry();
        registry.Register(new InjuryDefinition(
            DefinitionId.From("injury.invalid"),
            Name: string.Empty,
            IntegrityLossPerSeverityPerSecond: -1,
            DefaultBleedingSeverity: BleedingSeverity.None));

        Assert.Throws<InvalidDefinitionDataException>(() =>
            new InjuryDefinitionValidator().Validate(registry));
    }

    [Fact]
    public void InjuryApplicationSystem_ReducesBodyPartIntegrity()
    {
        var world = CreateWorld();
        var entity = CreateBodyEntity(world);
        entity.AddComponent(new InjuryComponent(new[]
        {
            new InjuryState(
                DefinitionReference<InjuryDefinition>.From("injury.test"),
                "core",
                Severity: 1)
        }));

        world.RegisterSystem(new InjuryApplicationSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var body = entity.GetComponent<BodyStateComponent>();
        var part = body.Parts.Single(part => part.PartId == "core");

        Assert.Equal(8, part.Integrity);
        Assert.Equal(0.8, entity.GetComponent<BodyIntegrityComponent>().NormalizedIntegrity);
    }

    [Fact]
    public void InjuryApplicationSystem_DoesNotRecoverSeverityAutomatically()
    {
        var world = CreateWorld();
        var entity = CreateBodyEntity(world);
        entity.AddComponent(new InjuryComponent(new[]
        {
            new InjuryState(
                DefinitionReference<InjuryDefinition>.From("injury.test"),
                "core",
                Severity: 1)
        }));

        world.RegisterSystem(new InjuryApplicationSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var injury = entity.GetComponent<InjuryComponent>().Injuries.Single();

        Assert.Equal(1, injury.Severity);
    }

    [Fact]
    public void BleedingSystem_ReducesBloodForBleedingInjuries()
    {
        var world = CreateWorld();
        var entity = CreateBodyEntity(world);
        entity.AddComponent(new BloodComponent(CurrentVolumeLiters: 5, MaxVolumeLiters: 5));
        entity.AddComponent(new InjuryComponent(new[]
        {
            new InjuryState(
                DefinitionReference<InjuryDefinition>.From("injury.test"),
                "core",
                Severity: 1,
                BleedingSeverity.Moderate)
        }));

        world.RegisterSystem(new BleedingSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.Equal(4.985, entity.GetComponent<BloodComponent>().CurrentVolumeLiters, precision: 3);
    }

    [Fact]
    public void NaturalRecoverySystem_UsesEntitySpecificRecovery()
    {
        var world = CreateWorld();
        var entity = CreateBodyEntity(world);
        entity.AddComponent(new NaturalRecoveryComponent(
            TissueRecoveryPerSecond: 0.2,
            BleedingRecoveryPerSecond: 0.1));
        entity.AddComponent(new InjuryComponent(new[]
        {
            new InjuryState(
                DefinitionReference<InjuryDefinition>.From("injury.test"),
                "core",
                Severity: 1,
                BleedingSeverity.Minor)
        }));

        world.RegisterSystem(new NaturalRecoverySystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var injury = entity.GetComponent<InjuryComponent>().Injuries.Single();

        Assert.Equal(0.7, injury.Severity, precision: 3);
    }

    [Fact]
    public void InjuryApplicationSystem_IgnoresEntitiesWithoutBodyState()
    {
        var world = CreateWorld();
        var entity = world.CreateEntity();
        entity.AddComponent(new InjuryComponent(new[]
        {
            new InjuryState(
                DefinitionReference<InjuryDefinition>.From("injury.test"),
                "core",
                Severity: 1)
        }));

        world.RegisterSystem(new InjuryApplicationSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.True(entity.HasComponent<InjuryComponent>());
        Assert.False(entity.HasComponent<BodyIntegrityComponent>());
    }

    [Fact]
    public void InjuryModule_RegistersSystems()
    {
        var world = CreateWorld();
        var module = new InjuryModule();

        module.RegisterSystems(world);

        Assert.Contains(world.Systems, system => system is InjuryApplicationSystem);
        Assert.Contains(world.Systems, system => system is BleedingSystem);
    }

    private static WorldState CreateWorld()
    {
        var registry = new DefinitionRegistry();
        registry.Register(new MaterialDefinition(
            DefinitionId.From("material.test"),
            Density: 1000,
            SpecificHeatCapacity: 1000,
            ThermalConductivity: 1));
        registry.Register(new BodyDefinition(
            DefinitionId.From("body.test"),
            new[]
            {
                new BodyPartDefinition(
                    "core",
                    "Core",
                    DefinitionReference<MaterialDefinition>.From("material.test"),
                    MaxIntegrity: 10)
            }));
        registry.Register(new InjuryDefinition(
            DefinitionId.From("injury.test"),
            Name: "Test injury",
            IntegrityLossPerSeverityPerSecond: 2,
            DefaultBleedingSeverity: BleedingSeverity.Moderate));
        registry.Freeze();

        return new WorldState(
            new SimulationTime(),
            new EventBus(),
            registry,
            NoOpTraceLogger.Instance);
    }

    private static Entity CreateBodyEntity(WorldState world)
    {
        var entity = world.CreateEntity();
        entity.AddComponent(new BodyStateComponent(
            DefinitionReference<BodyDefinition>.From("body.test"),
            new[]
            {
                new BodyPartState("core", 10, 10)
            }));

        return entity;
    }
}
