using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Modules.Materials;
using Isekai.Engine.Modules.Temperature;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for the universal temperature module.
/// </summary>
public sealed class TemperatureModuleTests
{
    [Fact]
    public void TemperatureExchangeSystem_WarmsEntityTowardAmbientTemperature()
    {
        var world = CreateWorldWithAmbient(20);
        var entity = CreateMaterialEntity(world, 0);

        world.RegisterSystem(new TemperatureExchangeSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var temperature = entity.GetComponent<TemperatureComponent>();

        Assert.True(temperature.Celsius > 0);
        Assert.True(temperature.Celsius < 20);
    }

    [Fact]
    public void TemperatureExchangeSystem_CoolsEntityTowardAmbientTemperature()
    {
        var world = CreateWorldWithAmbient(10);
        var entity = CreateMaterialEntity(world, 40);

        world.RegisterSystem(new TemperatureExchangeSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var temperature = entity.GetComponent<TemperatureComponent>();

        Assert.True(temperature.Celsius < 40);
        Assert.True(temperature.Celsius > 10);
    }

    [Fact]
    public void ThermalComfortSystem_AddsComfortOnlyToSensitiveEntities()
    {
        var world = CreateWorldWithAmbient(20);
        var sensitive = CreateMaterialEntity(world, 37);
        var insensitive = CreateMaterialEntity(world, 12);

        sensitive.AddComponent(new ThermalSensitivityComponent(36, 38));

        world.RegisterSystem(new ThermalComfortSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.True(sensitive.HasComponent<ThermalComfortComponent>());
        Assert.False(insensitive.HasComponent<ThermalComfortComponent>());
    }

    [Fact]
    public void ThermalComfortSystem_ReportsColdStress()
    {
        var world = CreateWorldWithAmbient(20);
        var entity = CreateMaterialEntity(world, 30);

        entity.AddComponent(new ThermalSensitivityComponent(36, 38));

        world.RegisterSystem(new ThermalComfortSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var comfort = entity.GetComponent<ThermalComfortComponent>();

        Assert.True(comfort.ColdStress > 0);
        Assert.Equal(0, comfort.HeatStress);
        Assert.True(comfort.Comfort < 1);
    }

    [Fact]
    public void TemperatureModule_RegistersTemperatureSystems()
    {
        var world = CreateWorldWithAmbient(20);
        var module = new TemperatureModule();

        module.RegisterSystems(world);

        Assert.Contains(world.Systems, system => system is TemperatureExchangeSystem);
        Assert.Contains(world.Systems, system => system is ThermalComfortSystem);
    }

    private static WorldState CreateWorldWithAmbient(double ambientCelsius)
    {
        var registry = new DefinitionRegistry();
        registry.Register(new MaterialDefinition(
            DefinitionId.From("material.test"),
            Density: 1000,
            SpecificHeatCapacity: 1000,
            ThermalConductivity: 100));
        registry.Freeze();

        var world = new WorldState(
            new SimulationTime(),
            new EventBus(),
            registry,
            NoOpTraceLogger.Instance);

        var ambient = world.CreateEntity();
        ambient.AddComponent(new AmbientTemperatureComponent(ambientCelsius));

        return world;
    }

    private static Entity CreateMaterialEntity(WorldState world, double temperatureCelsius)
    {
        var entity = world.CreateEntity();
        entity.AddComponent(new TemperatureComponent(temperatureCelsius));
        entity.AddComponent(new MaterialCompositionComponent(new[]
        {
            new MaterialQuantity(
                DefinitionReference<MaterialDefinition>.From("material.test"),
                Volume: 1)
        }));

        return entity;
    }
}
