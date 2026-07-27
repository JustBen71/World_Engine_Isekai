using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Interfaces;
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
        var world = CreateWorldWithAmbient(30);
        var entity = CreateMaterialEntity(world, 10);

        world.RegisterSystem(new TemperatureExchangeSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var temperature = entity.GetComponent<TemperatureComponent>();

        Assert.True(temperature.Celsius > 10);
        Assert.True(temperature.Celsius <= 30);
    }

    [Fact]
    public void TemperatureExchangeSystem_CoolsEntityTowardAmbientTemperature()
    {
        var world = CreateWorldWithAmbient(0);
        var entity = CreateMaterialEntity(world, 30);

        world.RegisterSystem(new TemperatureExchangeSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var temperature = entity.GetComponent<TemperatureComponent>();

        Assert.True(temperature.Celsius < 30);
        Assert.True(temperature.Celsius >= 0);
    }

    [Fact]
    public void TemperatureExchangeSystem_DoesNotOvershootAmbientTemperature()
    {
        var world = CreateWorldWithAmbient(30);
        var entity = CreateMaterialEntity(world, 10);

        world.RegisterSystem(new TemperatureExchangeSystem());
        world.Tick(TimeSpan.FromSeconds(1_000));

        var temperature = entity.GetComponent<TemperatureComponent>();

        Assert.Equal(30, temperature.Celsius);
    }

    [Fact]
    public void TemperatureExchangeSystem_IgnoresEntityWithoutTemperature()
    {
        var world = CreateWorldWithAmbient(30);
        var entity = world.CreateEntity();
        entity.AddComponent(new AmbientTemperatureComponent(10));

        world.RegisterSystem(new TemperatureExchangeSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.False(entity.HasComponent<TemperatureComponent>());
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
    public void TemperatureExchangeSystem_DoesNotCreateComfortForInsensitiveEntity()
    {
        var world = CreateWorldWithAmbient(30);
        var entity = CreateMaterialEntity(world, 10);

        world.RegisterSystem(new TemperatureExchangeSystem());
        world.RegisterSystem(new ThermalComfortSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.True(entity.GetComponent<TemperatureComponent>().Celsius > 10);
        Assert.False(entity.HasComponent<ThermalComfortComponent>());
    }

    [Fact]
    public void ThermalComfortSystem_CreatesCoherentComfortForSensitiveEntity()
    {
        var world = CreateWorldWithAmbient(20);
        var entity = CreateMaterialEntity(world, 34);
        entity.AddComponent(new ThermalSensitivityComponent(36, 38));

        world.RegisterSystem(new ThermalComfortSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var comfort = entity.GetComponent<ThermalComfortComponent>();

        Assert.InRange(comfort.Comfort, 0, 1);
        Assert.True(comfort.ColdStress > 0);
        Assert.Equal(0, comfort.HeatStress);
    }

    [Fact]
    public void TemperatureExchangeSystem_UsesCustomAmbientProvider()
    {
        var world = CreateWorldWithAmbient(-50);
        var entity = CreateMaterialEntity(world, 10);

        world.RegisterSystem(new TemperatureExchangeSystem(new FixedAmbientTemperatureProvider(40)));
        world.Tick(TimeSpan.FromSeconds(1));

        var temperature = entity.GetComponent<TemperatureComponent>();

        Assert.True(temperature.Celsius > 10);
        Assert.True(temperature.Celsius <= 40);
    }

    [Fact]
    public void ComponentAmbientTemperatureProvider_ReproducesExistingAmbientComponentBehavior()
    {
        var world = CreateWorldWithAmbient(26.5);
        var entity = CreateMaterialEntity(world, 10);
        var provider = new ComponentAmbientTemperatureProvider();

        var ambient = provider.GetAmbientTemperatureCelsius(world, entity);

        Assert.Equal(26.5, ambient);
    }

    [Fact]
    public void TemperatureExchangeSystem_IsDeterministic()
    {
        var first = CreateWorldWithAmbient(10);
        var second = CreateWorldWithAmbient(10);
        var firstEntity = CreateMaterialEntity(first, 5);
        var secondEntity = CreateMaterialEntity(second, 5);

        first.RegisterSystem(new TemperatureExchangeSystem(new FixedAmbientTemperatureProvider(30)));
        second.RegisterSystem(new TemperatureExchangeSystem(new FixedAmbientTemperatureProvider(30)));

        for (var i = 0; i < 10; i++)
        {
            first.Tick(TimeSpan.FromSeconds(0.5));
            second.Tick(TimeSpan.FromSeconds(0.5));
        }

        Assert.Equal(
            firstEntity.GetComponent<TemperatureComponent>().Celsius,
            secondEntity.GetComponent<TemperatureComponent>().Celsius);
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

    [Fact]
    public void TemperatureModule_AcceptsExplicitAmbientProvider()
    {
        var world = CreateWorldWithAmbient(0);
        var entity = CreateMaterialEntity(world, 10);
        var module = new TemperatureModule(new FixedAmbientTemperatureProvider(30));

        module.RegisterSystems(world);
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.True(entity.GetComponent<TemperatureComponent>().Celsius > 10);
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

    private sealed class FixedAmbientTemperatureProvider : IAmbientTemperatureProvider
    {
        private readonly double _ambientTemperatureCelsius;

        public FixedAmbientTemperatureProvider(double ambientTemperatureCelsius)
        {
            _ambientTemperatureCelsius = ambientTemperatureCelsius;
        }

        public double GetAmbientTemperatureCelsius(IWorldState world, Entity entity)
        {
            return _ambientTemperatureCelsius;
        }
    }
}
