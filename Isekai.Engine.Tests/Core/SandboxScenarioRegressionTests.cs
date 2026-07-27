using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Core.Modules;
using Isekai.Engine.Data.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Modules.Body;
using Isekai.Engine.Modules.BodyCapabilities;
using Isekai.Engine.Modules.Composition;
using Isekai.Engine.Modules.Handling;
using Isekai.Engine.Modules.Healing;
using Isekai.Engine.Modules.Injuries;
using Isekai.Engine.Modules.Impact;
using Isekai.Engine.Modules.Materials;
using Isekai.Engine.Modules.Temperature;
using Isekai.Engine.Modules.Vitals;
using Isekai.Engine.Sandbox.Components;
using Isekai.Engine.Sandbox.Data.Entities;
using Isekai.Engine.Sandbox.Systems;
using Isekai.Engine.Sandbox.World;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Regression tests that execute the same JSON scenarios exposed by the console sandbox.
/// </summary>
public sealed class SandboxScenarioRegressionTests
{
    [Fact]
    public void WoodcuttingScenario_CutsYoungTreeWithoutDestroyingAxe()
    {
        var world = CreateScenarioWorld("woodcutting.json");

        Tick(world, 30);

        var tree = FindByName(world, "Jeune arbre");
        var axe = FindByName(world, "Hache os-silex");

        Assert.False(tree.GetComponent<VitalStateComponent>().IsAlive);
        Assert.NotEmpty(tree.GetComponent<InjuryComponent>().Injuries);
        Assert.True(axe.GetComponent<CompositeIntegrityComponent>().NormalizedIntegrity > 0.90);
    }

    [Fact]
    public void DeerKicksHumanScenario_InjuresHumanLegWithoutInstantKill()
    {
        var world = CreateScenarioWorld("deer_kicks_human.json");

        Tick(world, 45);

        var human = FindByName(world, "Humain");
        var deer = FindByName(world, "Cerf");
        var vital = human.GetComponent<VitalStateComponent>();
        var blood = human.GetComponent<BloodComponent>();

        Assert.True(vital.IsAlive);
        Assert.True(blood.Ratio < 1);
        Assert.Contains(human.GetComponent<InjuryComponent>().Injuries, injury =>
            injury.BodyPartId == "right_leg" &&
            injury.Injury.Id.Value == "injury.generic.deep_cut");
        Assert.Contains(deer.GetComponent<BodyContactSurfacesComponent>().Surfaces, surface =>
            surface.Role == "front_hoof" &&
            surface.BodyPartId == "front_legs");
        Assert.DoesNotContain(world.Entities, entity =>
            entity.TryGetComponent<NameComponent>(out var name) &&
            name is not null &&
            string.Equals(name.Name, "Sabot du cerf", StringComparison.Ordinal));
    }

    [Fact]
    public void FreeMovementScenario_MovesMobileEntitiesOnly()
    {
        var world = CreateScenarioWorld("free_movement.json");

        Tick(world, 5);

        Assert.Equal(new Position2DComponent(6, 1), FindByName(world, "Humain mobile").GetComponent<Position2DComponent>());
        Assert.Equal(new Position2DComponent(9, 0), FindByName(world, "Cerf mobile").GetComponent<Position2DComponent>());
        Assert.Equal(new Position2DComponent(8, 4), FindByName(world, "Pierre immobile").GetComponent<Position2DComponent>());
    }

    [Fact]
    public void SteelAxeScenario_CutsTreeWithVeryLowHeadWear()
    {
        var world = CreateScenarioWorld("steel_axe_vs_tree.json");

        Tick(world, 25);

        var tree = FindByName(world, "Jeune arbre");
        var steelHead = FindByName(world, "Tete acier");

        Assert.False(tree.GetComponent<VitalStateComponent>().IsAlive);
        Assert.True(steelHead.GetComponent<BodyIntegrityComponent>().NormalizedIntegrity > 0.95);
    }

    [Fact]
    public void BleedingScenario_KillsByBloodLoss()
    {
        var world = CreateScenarioWorld("bleeding_until_death.json");

        Tick(world, 60);

        var human = FindByName(world, "Humain hemorragie");
        var vital = human.GetComponent<VitalStateComponent>();

        Assert.False(vital.IsAlive);
        Assert.Equal("blood_loss", vital.DeathReason);
    }

    [Fact]
    public void NaturalRecoveryScenario_RemovesMinorDeerInjury()
    {
        var world = CreateScenarioWorld("natural_recovery.json");

        Tick(world, 40);

        var deer = FindByName(world, "Cerf en recuperation");

        Assert.Empty(deer.GetComponent<InjuryComponent>().Injuries);
    }

    [Fact]
    public void TemperatureComfortScenario_ReportsColdStress()
    {
        var world = CreateScenarioWorld("temperature_comfort.json");

        Tick(world, 3);

        Assert.True(FindByName(world, "Humain refroidi").GetComponent<ThermalComfortComponent>().ColdStress > 0);
        Assert.True(FindByName(world, "Cerf refroidi").GetComponent<ThermalComfortComponent>().ColdStress > 0);
    }

    [Fact]
    public void CompositeWearScenario_WearsContactPartAndComposite()
    {
        var world = CreateScenarioWorld("composite_wear.json");

        Tick(world, 30);

        var flint = FindByName(world, "Silex taille");
        var axe = FindByName(world, "Hache os-silex");

        Assert.True(flint.GetComponent<BodyIntegrityComponent>().NormalizedIntegrity < 1);
        Assert.True(axe.GetComponent<CompositeIntegrityComponent>().NormalizedIntegrity < 1);
    }

    [Fact]
    public void ThermalMaterialsScenario_MovesTemperaturesTowardAmbient()
    {
        var world = CreateScenarioWorld("thermal_materials.json");

        Tick(world, 10);

        Assert.True(FindByName(world, "Eau froide").GetComponent<TemperatureComponent>().Celsius > 4.0);
        Assert.True(FindByName(world, "Fer chaud").GetComponent<TemperatureComponent>().Celsius < 80.0);
        Assert.True(FindByName(world, "Pierre tiede").GetComponent<TemperatureComponent>().Celsius < 28.0);
    }

    [Fact]
    public void ThermalZonesScenario_UsesLocalAmbientTemperatures()
    {
        var world = CreateScenarioWorld(
            "thermal_zones.json",
            new SandboxZoneAmbientTemperatureProvider(30));

        Tick(world, 30);

        var cold = FindByName(world, "Temoin zone froide").GetComponent<TemperatureComponent>().Celsius;
        var temperate = FindByName(world, "Temoin zone temperee").GetComponent<TemperatureComponent>().Celsius;
        var hot = FindByName(world, "Temoin zone chaude").GetComponent<TemperatureComponent>().Celsius;

        Assert.True(cold < temperate);
        Assert.True(temperate < hot);
        Assert.True(cold < 20);
        Assert.InRange(temperate, 19.5, 20.5);
        Assert.True(hot > 20);
    }

    private static WorldState CreateScenarioWorld(
        string scenarioFileName,
        IAmbientTemperatureProvider? ambientTemperatureProvider = null)
    {
        var modules = CreateModules(ambientTemperatureProvider);
        var definitions = LoadDefinitions(modules);
        var map = new SandboxMap(30, 7);
        var world = new WorldState(
            new SimulationTime(),
            new EventBus(),
            definitions,
            NoOpTraceLogger.Instance);

        var ambient = world.CreateEntity();
        ambient.AddComponent(new NameComponent("Ambiance"));
        ambient.AddComponent(new AmbientTemperatureComponent(map.GetTemperatureStats(world.Time.TickCount).Average));

        world.RegisterSystem(new MapAmbientTemperatureSystem(map));
        foreach (var module in modules)
        {
            module.RegisterSystems(world);
        }

        world.RegisterSystem(new Movement2DSystem(map.Width, map.Height));

        new SandboxEntityLoader().LoadFileInto(
            Path.Combine(FindRepositoryRoot(), "Isekai.Engine.Sandbox", "Data", "scenarios", scenarioFileName),
            world);

        return world;
    }

    private static IReadOnlyCollection<IEngineModule> CreateModules(
        IAmbientTemperatureProvider? ambientTemperatureProvider = null)
    {
        return new IEngineModule[]
        {
            new BodyModule(),
            new InjuryModule(),
            new HealingModule(),
            new MaterialModule(),
            new CompositionModule(),
            new HandlingModule(),
            new BodyCapabilitiesModule(),
            new ImpactModule(),
            new VitalsModule(),
            new TemperatureModule(ambientTemperatureProvider ?? new ComponentAmbientTemperatureProvider())
        };
    }

    private static DefinitionRegistry LoadDefinitions(IReadOnlyCollection<IEngineModule> modules)
    {
        var typeRegistry = new DefinitionTypeRegistry(NoOpTraceLogger.Instance);
        foreach (var module in modules)
        {
            module.RegisterDefinitionTypes(typeRegistry);
        }

        var validators = modules.SelectMany(module => module.CreateDefinitionValidators()).ToArray();
        var pipeline = new DefinitionLoadPipeline(
            new JsonDefinitionLoader(typeRegistry, NoOpTraceLogger.Instance),
            validators,
            NoOpTraceLogger.Instance);

        return pipeline.LoadDirectory(Path.Combine(FindRepositoryRoot(), "Isekai.Engine.Sandbox", "Data", "definitions"));
    }

    private static Entity FindByName(WorldState world, string name)
    {
        return world.Entities.Single(entity =>
            entity.TryGetComponent<NameComponent>(out var component) &&
            component is not null &&
            string.Equals(component.Name, name, StringComparison.Ordinal));
    }

    private static void Tick(WorldState world, int ticks)
    {
        for (var i = 0; i < ticks; i++)
        {
            world.Tick(TimeSpan.FromSeconds(1));
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var scenarioDirectory = Path.Combine(directory.FullName, "Isekai.Engine.Sandbox", "Data", "scenarios");
            if (Directory.Exists(scenarioDirectory))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root from test output directory.");
    }
}
