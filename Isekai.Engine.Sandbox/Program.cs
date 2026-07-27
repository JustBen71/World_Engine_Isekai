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
using Isekai.Engine.Sandbox.Terminal;
using Isekai.Engine.Sandbox.World;
using Spectre.Console;

const int width = 30;
const int height = 7;
var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data", "definitions");
var scenarioDirectory = Path.Combine(AppContext.BaseDirectory, "Data", "scenarios");
var scenario = SelectScenario(args);
var tickCount = ReadTickCount(args, scenario.DefaultTicks);
var entityFile = Path.Combine(scenarioDirectory, scenario.EntityFileName);

var logger = CreateLogger(args);
try
{
    AnsiConsole.MarkupLine($"[bold green]Scenario:[/] {Markup.Escape(scenario.Name)}");
    AnsiConsole.MarkupLine($"[grey]{Markup.Escape(scenario.Description)}[/]");

    var map = new SandboxMap(width, height);
    var modules = CreateModules(scenario, map);
    var definitions = LoadDefinitions(dataDirectory, modules, logger);
    var terrainService = CreateTerrainService(definitions);
    var world = CreateWorld(definitions, logger, map, modules, entityFile, scenario, terrainService);
    var renderer = new SpectreWorldRenderer(map, frameDelayMilliseconds: 250);

    if (scenario.ShowTerrainDiagnostics)
    {
        RenderTerrainDiagnostics(world, terrainService);
    }

    if (scenario.ShowNeedsDiagnostics)
    {
        RenderNeedsDiagnostics("Before", world);
    }

    if (scenario.EnableMovementResolution)
    {
        RenderMovementPerceptionDiagnostics("Before", world);
    }

    renderer.Run(world, tickCount);

    if (scenario.ShowNeedsDiagnostics)
    {
        RenderNeedsDiagnostics("After", world);
    }

    if (scenario.EnableMovementResolution)
    {
        RenderMovementPerceptionDiagnostics("After", world);
    }
}
finally
{
    if (logger is IDisposable disposableLogger)
    {
        disposableLogger.Dispose();
    }
}

static SandboxScenario SelectScenario(string[] args)
{
    var scenarioOption = args.FirstOrDefault(arg => arg.StartsWith("--scenario=", StringComparison.OrdinalIgnoreCase));
    if (scenarioOption is not null)
    {
        var id = scenarioOption["--scenario=".Length..];
        return SandboxScenarioRegistry.FindById(id) ??
               throw new ArgumentException($"Unknown sandbox scenario '{id}'.");
    }

    if (!AnsiConsole.Profile.Capabilities.Interactive)
    {
        return SandboxScenarioRegistry.FindById("woodcutting") ??
               throw new InvalidOperationException("Default sandbox scenario is missing.");
    }

    return AnsiConsole.Prompt(
        new SelectionPrompt<SandboxScenario>()
            .Title("[bold]Choisis le scenario sandbox a lancer[/]")
            .PageSize(10)
            .UseConverter(scenario => $"{scenario.Name} [grey]({scenario.Id})[/]")
            .AddChoices(SandboxScenarioRegistry.All));
}

static int ReadTickCount(string[] args, int defaultTickCount)
{
    var ticksOption = args.FirstOrDefault(arg => arg.StartsWith("--ticks=", StringComparison.OrdinalIgnoreCase));
    if (ticksOption is null)
    {
        return defaultTickCount;
    }

    var value = ticksOption["--ticks=".Length..];
    return int.TryParse(value, out var tickCount) && tickCount > 0 ? tickCount : defaultTickCount;
}

static ITraceLogger CreateLogger(string[] args)
{
    var enableTrace = args.Any(arg => string.Equals(arg, "--trace", StringComparison.OrdinalIgnoreCase));
    return enableTrace
        ? new FileTraceLogger("debug.log", EngineLogLevels.FullTrace)
        : NoOpTraceLogger.Instance;
}

static IReadOnlyCollection<IEngineModule> CreateModules(SandboxScenario scenario, SandboxMap map)
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

static DefinitionRegistry LoadDefinitions(
    string dataDirectory,
    IReadOnlyCollection<IEngineModule> modules,
    ITraceLogger logger)
{
    var types = new DefinitionTypeRegistry(logger);
    foreach (var module in modules)
    {
        module.RegisterDefinitionTypes(types);
    }

    var validators = modules.SelectMany(module => module.CreateDefinitionValidators()).ToArray();

    var pipeline = new DefinitionLoadPipeline(
        new JsonDefinitionLoader(types, logger),
        validators,
        logger);

    return pipeline.LoadDirectory(dataDirectory);
}

static ITerrainService CreateTerrainService(DefinitionRegistry definitions)
{
    var definition = definitions.Get<TerrainGridDefinition>(DefinitionId.From("terrain.demo.basic"));
    var grid = new SimpleTerrainGenerator().Generate(new TerrainGenerationRequest(definition));

    grid.SetSoil(new TerrainCellCoordinate(7, 1), DefinitionId.From("soil.gravel"));
    grid.SetSoil(new TerrainCellCoordinate(8, 8), DefinitionId.From("soil.clay"));

    return new TerrainService(grid);
}

static WorldState CreateWorld(
    DefinitionRegistry definitions,
    ITraceLogger logger,
    SandboxMap map,
    IReadOnlyCollection<IEngineModule> modules,
    string entityFile,
    SandboxScenario scenario,
    ITerrainService terrainService)
{
    var world = new WorldState(
        new SimulationTime(logger),
        new EventBus(logger),
        definitions,
        logger);

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
        var observationBuilder = new AgentObservationBuilder();
        world.RegisterSystem(new SandboxPerceptionMoveControllerSystem(observationBuilder));
    }

    if (scenario.EnableMovementResolution)
    {
        world.RegisterSystem(new MovementResolutionSystem(terrainService));
        world.RegisterSystem(new SandboxPosition2DSyncSystem(map.Width, map.Height));
    }

    world.RegisterSystem(new Movement2DSystem(map.Width, map.Height));

    new SandboxEntityLoader().LoadFileInto(entityFile, world);

    return world;
}

static void RenderMovementPerceptionDiagnostics(string label, WorldState world)
{
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine($"[bold yellow]Movement/Perception diagnostics - {Markup.Escape(label)}[/]");
    var builder = new AgentObservationBuilder();

    foreach (var entity in world.Entities
                 .Where(entity =>
                     entity.HasComponent<MovementCapabilityComponent>() ||
                     entity.HasComponent<MovementResultComponent>() ||
                     entity.HasComponent<PerceptionCapabilityComponent>()))
    {
        var name = entity.TryGetComponent<NameComponent>(out var nameComponent) && nameComponent is not null
            ? nameComponent.Name
            : entity.Id.ToString();
        var position = entity.TryGetComponent<PositionComponent>(out var positionComponent) && positionComponent is not null
            ? positionComponent.Position
            : new WorldPosition(0, 0, 0);
        var target = "none";
        if (entity.HasComponent<PerceptionCapabilityComponent>() && entity.HasComponent<PositionComponent>())
        {
            var observation = builder.BuildObservation(world, entity.Id);
            var first = observation.PerceivedEntities.FirstOrDefault();
            target = first is null
                ? "none"
                : $"{string.Join(",", first.Tags)} at {first.DistanceMeters:0.##}m";
        }

        var movement = entity.TryGetComponent<MovementResultComponent>(out var result) && result is not null
            ? $"{result.Result.Outcome} moved={result.Result.ActualDistanceMeters:0.##}m"
            : "no result";

        AnsiConsole.MarkupLine($"{Markup.Escape(name)} pos=({position.X:0.##},{position.Y:0.##},{position.Z:0.##}) sees={Markup.Escape(target)} move={Markup.Escape(movement)}");
    }

    AnsiConsole.WriteLine();
}

static void RenderTerrainDiagnostics(WorldState world, ITerrainService terrainService)
{
    var grid = terrainService.Grid;
    var center = new TerrainCellCoordinate(5, 5);
    var centerCell = grid.GetCell(center);
    var neighbors = string.Join(", ", grid.GetNeighbors8(center).Select(coordinate => $"({coordinate.X},{coordinate.Y})"));

    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold yellow]Terrain diagnostics[/]");
    AnsiConsole.MarkupLine($"Grid: {grid.WidthInCells}x{grid.HeightInCells}, cell={grid.CellSizeMeters:0.##}m");
    AnsiConsole.MarkupLine($"Center: ({center.X},{center.Y}) elevation={centerCell.ElevationMeters:0.##}m soil={Markup.Escape(centerCell.SoilDefinitionId.Value)}");
    AnsiConsole.MarkupLine($"Neighbors8: {Markup.Escape(neighbors)}");

    foreach (var entity in world.EntitiesWith<PositionComponent>())
    {
        var name = entity.TryGetComponent<NameComponent>(out var nameComponent) && nameComponent is not null
            ? nameComponent.Name
            : entity.Id.Value.ToString();
        var position = entity.GetComponent<PositionComponent>().Position;
        var coordinate = terrainService.GetCellAt(position);
        var cell = grid.GetCell(coordinate);

        AnsiConsole.MarkupLine(
            $"{Markup.Escape(name)} -> position=({position.X:0.##},{position.Y:0.##},{position.Z:0.##}) cell=({coordinate.X},{coordinate.Y}) elevation={cell.ElevationMeters:0.##}m soil={Markup.Escape(cell.SoilDefinitionId.Value)}");
    }

    AnsiConsole.WriteLine();
}

static void RenderNeedsDiagnostics(string label, WorldState world)
{
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine($"[bold yellow]Needs diagnostics - {Markup.Escape(label)}[/]");

    foreach (var entity in world.Entities)
    {
        if (!entity.TryGetComponent<NameComponent>(out var nameComponent) || nameComponent is null)
        {
            continue;
        }

        var parts = new List<string>();
        if (entity.TryGetComponent<EnergyNeedComponent>(out var energy) && energy is not null)
        {
            parts.Add($"E={energy.CurrentEnergy:0.##}/{energy.MaximumEnergy:0.##}");
        }

        if (entity.TryGetComponent<HydrationNeedComponent>(out var hydration) && hydration is not null)
        {
            parts.Add($"H={hydration.CurrentHydration:0.##}/{hydration.MaximumHydration:0.##}");
        }

        if (entity.TryGetComponent<ConsumableResourceComponent>(out var consumable) && consumable is not null)
        {
            parts.Add($"Food={consumable.CurrentQuantity:0.##}/{consumable.MaximumQuantity:0.##}");
        }

        if (entity.TryGetComponent<WaterSourceComponent>(out var water) && water is not null)
        {
            parts.Add($"Water={water.CurrentVolumeLiters:0.##}/{water.MaximumVolumeLiters:0.##}");
        }

        if (entity.TryGetComponent<ConsumeActionResultComponent>(out var consumeResult) && consumeResult is not null)
        {
            parts.Add($"Consume={consumeResult.Result.Outcome} q={consumeResult.Result.ConsumedQuantity:0.##}");
        }

        if (entity.TryGetComponent<DrinkActionResultComponent>(out var drinkResult) && drinkResult is not null)
        {
            parts.Add($"Drink={drinkResult.Result.Outcome} q={drinkResult.Result.ConsumedVolumeLiters:0.##} exp={drinkResult.Result.BacterialExposure + drinkResult.Result.ChemicalExposure + drinkResult.Result.SalinityExposure:0.##}");
        }

        if (entity.TryGetComponent<ContaminationExposureComponent>(out var exposure) && exposure is not null)
        {
            parts.Add($"Exposures={exposure.Exposures.Count}");
        }

        if (parts.Count > 0)
        {
            AnsiConsole.MarkupLine($"{Markup.Escape(nameComponent.Name)} -> {Markup.Escape(string.Join(" | ", parts))}");
        }
    }

    AnsiConsole.WriteLine();
}
