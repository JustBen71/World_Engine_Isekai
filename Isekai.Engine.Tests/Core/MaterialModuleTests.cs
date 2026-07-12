using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Data.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Exceptions;
using Isekai.Engine.Modules.Materials;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for the universal material module.
/// </summary>
public sealed class MaterialModuleTests
{
    [Fact]
    public void RegisterMaterialDefinitions_RegistersMaterialDefinitionType()
    {
        var types = new DefinitionTypeRegistry();

        types.RegisterMaterialDefinitions();

        Assert.Contains("material.definition", types.TypeNames);
    }

    [Fact]
    public void JsonLoader_LoadsMaterialDefinition()
    {
        var types = new DefinitionTypeRegistry();
        types.RegisterMaterialDefinitions();
        var loader = new JsonDefinitionLoader(types);

        const string json = """
        {
          "type": "material.definition",
          "id": "material.test",
          "density": 1000,
          "specificHeatCapacity": 4200,
          "thermalConductivity": 0.6
        }
        """;

        var registry = loader.LoadJson(json);
        var material = registry.Get<MaterialDefinition>(DefinitionId.From("material.test"));

        Assert.Equal(1000, material.Density);
        Assert.Equal(4200, material.SpecificHeatCapacity);
        Assert.Equal(0.6, material.ThermalConductivity);
    }

    [Fact]
    public void MaterialDefinitionValidator_AcceptsValidMaterials()
    {
        var registry = new DefinitionRegistry();
        registry.Register(new MaterialDefinition(
            DefinitionId.From("material.valid"),
            Density: 1000,
            SpecificHeatCapacity: 4200,
            ThermalConductivity: 0.6));

        new MaterialDefinitionValidator().Validate(registry);
    }

    [Fact]
    public void MaterialDefinitionValidator_RejectsInvalidMaterials()
    {
        var registry = new DefinitionRegistry();
        registry.Register(new MaterialDefinition(
            DefinitionId.From("material.invalid"),
            Density: 0,
            SpecificHeatCapacity: -1,
            ThermalConductivity: -0.1));

        Assert.Throws<InvalidDefinitionDataException>(() =>
            new MaterialDefinitionValidator().Validate(registry));
    }

    [Fact]
    public void MaterialMassSystem_ComputesMassFromComposition()
    {
        var registry = CreateMaterialRegistry();
        var world = CreateWorld(registry);
        var entity = world.CreateEntity();

        entity.AddComponent(new MaterialCompositionComponent(new[]
        {
            new MaterialQuantity(
                DefinitionReference<MaterialDefinition>.From("material.water"),
                Volume: 2),
            new MaterialQuantity(
                DefinitionReference<MaterialDefinition>.From("material.iron"),
                Volume: 0.5)
        }));

        world.RegisterSystem(new MaterialMassSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var mass = entity.GetComponent<MaterialMassComponent>();

        Assert.Equal(5935, mass.Kilograms);
    }

    [Fact]
    public void MaterialMassSystem_ReplacesExistingMassComponent()
    {
        var registry = CreateMaterialRegistry();
        var world = CreateWorld(registry);
        var entity = world.CreateEntity();

        entity.AddComponent(new MaterialCompositionComponent(new[]
        {
            new MaterialQuantity(
                DefinitionReference<MaterialDefinition>.From("material.water"),
                Volume: 1)
        }));
        entity.AddComponent(new MaterialMassComponent(123));

        world.RegisterSystem(new MaterialMassSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.Equal(1000, entity.GetComponent<MaterialMassComponent>().Kilograms);
    }

    [Fact]
    public void MaterialMassSystem_ThrowsWhenCompositionReferencesMissingMaterial()
    {
        var registry = CreateMaterialRegistry();
        var world = CreateWorld(registry);
        var entity = world.CreateEntity();

        entity.AddComponent(new MaterialCompositionComponent(new[]
        {
            new MaterialQuantity(
                DefinitionReference<MaterialDefinition>.From("material.missing"),
                Volume: 1)
        }));

        world.RegisterSystem(new MaterialMassSystem());

        Assert.Throws<DefinitionNotFoundException>(() => world.Tick(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void MaterialMassSystem_ThrowsWhenVolumeIsNegative()
    {
        var registry = CreateMaterialRegistry();
        var world = CreateWorld(registry);
        var entity = world.CreateEntity();

        entity.AddComponent(new MaterialCompositionComponent(new[]
        {
            new MaterialQuantity(
                DefinitionReference<MaterialDefinition>.From("material.water"),
                Volume: -1)
        }));

        world.RegisterSystem(new MaterialMassSystem());

        Assert.Throws<InvalidMaterialCompositionException>(() => world.Tick(TimeSpan.FromSeconds(1)));
    }

    private static DefinitionRegistry CreateMaterialRegistry()
    {
        var registry = new DefinitionRegistry();
        registry.Register(new MaterialDefinition(
            DefinitionId.From("material.water"),
            Density: 1000,
            SpecificHeatCapacity: 4200,
            ThermalConductivity: 0.6));
        registry.Register(new MaterialDefinition(
            DefinitionId.From("material.iron"),
            Density: 7870,
            SpecificHeatCapacity: 450,
            ThermalConductivity: 80));

        registry.EnsureReferencesValid();
        registry.Freeze();

        return registry;
    }

    private static WorldState CreateWorld(DefinitionRegistry registry)
    {
        return new WorldState(
            new SimulationTime(),
            new EventBus(),
            registry,
            NoOpTraceLogger.Instance);
    }
}
