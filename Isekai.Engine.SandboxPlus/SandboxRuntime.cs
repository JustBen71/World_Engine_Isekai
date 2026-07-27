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
using Isekai.Engine.Modules.Impact;
using Isekai.Engine.Modules.Injuries;
using Isekai.Engine.Modules.Materials;
using Isekai.Engine.Modules.Movement;
using Isekai.Engine.Modules.Needs;
using Isekai.Engine.Modules.Perception;
using Isekai.Engine.Modules.Temperature;
using Isekai.Engine.Modules.Terrain;
using Isekai.Engine.Modules.Vitals;
using Isekai.Engine.Sandbox.Components;
using Isekai.Engine.Sandbox.Data.Entities;
using Isekai.Engine.Sandbox.Scenarios;
using Isekai.Engine.Sandbox.Systems;
using Isekai.Engine.Sandbox.World;

namespace Isekai.Engine.SandboxPlus;

/// <summary>
/// Builds sandbox worlds for the graphical viewer without adding UI concerns to the engine.
/// </summary>
internal static class SandboxRuntime
{
    public const int Width = 30;
    public const int Height = 7;

    public static SandboxSession CreateSession(SandboxScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        var map = new SandboxMap(Width, Height);
        var modules = CreateModules(scenario, map);
        var definitions = LoadDefinitions(modules);
        var terrainService = CreateTerrainService(definitions);
        var world = CreateWorld(definitions, map, modules, scenario, terrainService);

        return new SandboxSession(scenario, map, terrainService, world);
    }

    private static IReadOnlyCollection<IEngineModule> CreateModules(SandboxScenario scenario, SandboxMap map)
    {
        IAmbientTemperatureProvider ambientProvider = scenario.UseLocalAmbientTemperatureProvider
            ? new SandboxZoneAmbientTemperatureProvider(map.Width)
            : new ComponentAmbientTemperatureProvider();

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
            new TerrainModule(),
            new NeedsModule(),
            new PerceptionModule(),
            new VitalsModule(),
            new TemperatureModule(ambientProvider)
        };
    }

    private static DefinitionRegistry LoadDefinitions(IReadOnlyCollection<IEngineModule> modules)
    {
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data", "definitions");
        var types = new DefinitionTypeRegistry(NoOpTraceLogger.Instance);

        foreach (var module in modules)
        {
            module.RegisterDefinitionTypes(types);
        }

        var validators = modules.SelectMany(module => module.CreateDefinitionValidators()).ToArray();
        var pipeline = new DefinitionLoadPipeline(
            new JsonDefinitionLoader(types, NoOpTraceLogger.Instance),
            validators,
            NoOpTraceLogger.Instance);

        return pipeline.LoadDirectory(dataDirectory);
    }

    private static ITerrainService CreateTerrainService(DefinitionRegistry definitions)
    {
        var definition = definitions.Get<TerrainGridDefinition>(DefinitionId.From("terrain.demo.basic"));
        var grid = new SimpleTerrainGenerator().Generate(new TerrainGenerationRequest(definition));

        grid.SetSoil(new TerrainCellCoordinate(7, 1), DefinitionId.From("soil.gravel"));
        grid.SetSoil(new TerrainCellCoordinate(8, 8), DefinitionId.From("soil.clay"));

        return new TerrainService(grid);
    }

    private static WorldState CreateWorld(
        DefinitionRegistry definitions,
        SandboxMap map,
        IReadOnlyCollection<IEngineModule> modules,
        SandboxScenario scenario,
        ITerrainService terrainService)
    {
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

        if (scenario.EnableMovementPerceptionDemo)
        {
            world.RegisterSystem(new SandboxPerceptionMoveControllerSystem(new AgentObservationBuilder()));
        }

        if (scenario.EnableMovementResolution)
        {
            world.RegisterSystem(new MovementResolutionSystem(terrainService));
            world.RegisterSystem(new SandboxPosition2DSyncSystem(map.Width, map.Height));
        }

        world.RegisterSystem(new Movement2DSystem(map.Width, map.Height));

        new SandboxEntityLoader().LoadFileInto(
            Path.Combine(AppContext.BaseDirectory, "Data", "scenarios", scenario.EntityFileName),
            world);

        return world;
    }
}

/// <summary>
/// Holds the runtime objects of the selected sandbox scenario.
/// </summary>
internal sealed record SandboxSession(
    SandboxScenario Scenario,
    SandboxMap Map,
    ITerrainService TerrainService,
    WorldState World);
