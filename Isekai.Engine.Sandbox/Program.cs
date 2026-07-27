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
    var world = CreateWorld(definitions, logger, map, modules, entityFile);
    var renderer = new SpectreWorldRenderer(map, frameDelayMilliseconds: 250);

    renderer.Run(world, tickCount);
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

static WorldState CreateWorld(
    DefinitionRegistry definitions,
    ITraceLogger logger,
    SandboxMap map,
    IReadOnlyCollection<IEngineModule> modules,
    string entityFile)
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

    world.RegisterSystem(new Movement2DSystem(map.Width, map.Height));

    new SandboxEntityLoader().LoadFileInto(entityFile, world);

    return world;
}
