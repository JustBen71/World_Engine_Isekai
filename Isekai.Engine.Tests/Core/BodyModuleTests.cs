using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Data.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Exceptions;
using Isekai.Engine.Modules.Body;
using Isekai.Engine.Modules.Materials;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for the universal body module.
/// </summary>
public sealed class BodyModuleTests
{
    [Fact]
    public void RegisterBodyDefinitions_RegistersBodyDefinitionType()
    {
        var types = new DefinitionTypeRegistry();

        types.RegisterBodyDefinitions();

        Assert.Contains("body.definition", types.TypeNames);
    }

    [Fact]
    public void JsonLoader_LoadsBodyDefinition()
    {
        var types = new DefinitionTypeRegistry();
        types.RegisterBodyDefinitions();
        var loader = new JsonDefinitionLoader(types);

        const string json = """
        {
          "type": "body.definition",
          "id": "body.test",
          "parts": [
            {
              "id": "core",
              "name": "Core",
              "material": "material.test",
              "maxIntegrity": 10
            }
          ]
        }
        """;

        var registry = loader.LoadJson(json);
        var definition = registry.Get<BodyDefinition>(DefinitionId.From("body.test"));

        Assert.Single(definition.Parts);
        Assert.Equal("core", definition.Parts[0].Id);
    }

    [Fact]
    public void BodyDefinitionValidator_AcceptsValidBody()
    {
        var registry = CreateRegistryWithBody(CreateValidBodyDefinition());

        new BodyDefinitionValidator().Validate(registry);
    }

    [Fact]
    public void BodyDefinitionValidator_RejectsInvalidBody()
    {
        var registry = CreateRegistryWithMaterial();
        registry.Register(new BodyDefinition(
            DefinitionId.From("body.invalid"),
            new[]
            {
                new BodyPartDefinition(
                    string.Empty,
                    string.Empty,
                    DefinitionReference<MaterialDefinition>.From("material.missing"),
                    0)
            }));

        Assert.Throws<InvalidDefinitionDataException>(() =>
            new BodyDefinitionValidator().Validate(registry));
    }

    [Fact]
    public void BodyInitializationSystem_CreatesRuntimeBodyState()
    {
        var world = CreateWorldWithBody();
        var entity = world.CreateEntity();
        entity.AddComponent(new BodyComponent(DefinitionReference<BodyDefinition>.From("body.test")));

        world.RegisterSystem(new BodyInitializationSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var state = entity.GetComponent<BodyStateComponent>();
        var integrity = entity.GetComponent<BodyIntegrityComponent>();

        Assert.Single(state.Parts);
        Assert.Equal(1, integrity.NormalizedIntegrity);
    }

    [Fact]
    public void BodyIntegritySystem_ComputesAggregateIntegrity()
    {
        var world = CreateWorldWithBody();
        var entity = world.CreateEntity();
        entity.AddComponent(new BodyStateComponent(
            DefinitionReference<BodyDefinition>.From("body.test"),
            new[]
            {
                new BodyPartState("core", 5, 10),
                new BodyPartState("shell", 10, 10)
            }));

        world.RegisterSystem(new BodyIntegritySystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.Equal(0.75, entity.GetComponent<BodyIntegrityComponent>().NormalizedIntegrity);
    }

    [Fact]
    public void BodyInitializationSystem_IgnoresEntitiesWithoutBodyComponent()
    {
        var world = CreateWorldWithBody();
        var entity = world.CreateEntity();

        world.RegisterSystem(new BodyInitializationSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.False(entity.HasComponent<BodyStateComponent>());
    }

    [Fact]
    public void BodyModule_RegistersSystems()
    {
        var world = CreateWorldWithBody();
        var module = new BodyModule();

        module.RegisterSystems(world);

        Assert.Contains(world.Systems, system => system is BodyInitializationSystem);
        Assert.Contains(world.Systems, system => system is BodyIntegritySystem);
    }

    private static DefinitionRegistry CreateRegistryWithBody(BodyDefinition body)
    {
        var registry = CreateRegistryWithMaterial();
        registry.Register(body);
        registry.Freeze();
        return registry;
    }

    private static DefinitionRegistry CreateRegistryWithMaterial()
    {
        var registry = new DefinitionRegistry();
        registry.Register(new MaterialDefinition(
            DefinitionId.From("material.test"),
            Density: 1000,
            SpecificHeatCapacity: 1000,
            ThermalConductivity: 1));

        return registry;
    }

    private static BodyDefinition CreateValidBodyDefinition()
    {
        return new BodyDefinition(
            DefinitionId.From("body.test"),
            new[]
            {
                new BodyPartDefinition(
                    "core",
                    "Core",
                    DefinitionReference<MaterialDefinition>.From("material.test"),
                    10)
            });
    }

    private static WorldState CreateWorldWithBody()
    {
        return new WorldState(
            new SimulationTime(),
            new EventBus(),
            CreateRegistryWithBody(CreateValidBodyDefinition()),
            NoOpTraceLogger.Instance);
    }
}
