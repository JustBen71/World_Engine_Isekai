using System.Globalization;
using Isekai.Engine.Core;
using Isekai.Engine.Modules.Body;
using Isekai.Engine.Modules.Composition;
using Isekai.Engine.Modules.Handling;
using Isekai.Engine.Modules.Injuries;
using Isekai.Engine.Modules.Impact;
using Isekai.Engine.Modules.Materials;
using Isekai.Engine.Modules.Temperature;
using Isekai.Engine.Modules.Vitals;
using Isekai.Engine.Sandbox.Components;
using Isekai.Engine.Sandbox.World;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Isekai.Engine.Sandbox.Terminal;

/// <summary>
/// Renders the sandbox world with Spectre.Console.
/// </summary>
public sealed class SpectreWorldRenderer
{
    private readonly SandboxMap _map;
    private readonly int _frameDelayMilliseconds;

    /// <summary>
    /// Creates a Spectre.Console renderer.
    /// </summary>
    public SpectreWorldRenderer(SandboxMap map, int frameDelayMilliseconds)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
        _frameDelayMilliseconds = Math.Max(0, frameDelayMilliseconds);
    }

    /// <summary>
    /// Runs the live rendering loop for a fixed number of ticks.
    /// </summary>
    public void Run(WorldState world, int tickCount)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (tickCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tickCount), tickCount, "Tick count must be greater than zero.");
        }

        Console.OutputEncoding = System.Text.Encoding.UTF8;

        if (AnsiConsole.Profile.Capabilities.Interactive)
        {
            AnsiConsole.Live(BuildLayout(world, 0))
                .AutoClear(false)
                .Start(context =>
                {
                    for (var frame = 1; frame <= tickCount; frame++)
                    {
                        world.Tick(TimeSpan.FromSeconds(1));
                        context.UpdateTarget(BuildLayout(world, frame));
                        context.Refresh();

                        if (frame < tickCount)
                        {
                            Thread.Sleep(_frameDelayMilliseconds);
                        }
                    }
                });
        }
        else
        {
            for (var frame = 1; frame <= tickCount; frame++)
            {
                world.Tick(TimeSpan.FromSeconds(1));
                AnsiConsole.Write(BuildLayout(world, frame));
            }
        }

        AnsiConsole.MarkupLine("[bold green]Sandbox completed.[/]");
    }

    private Layout BuildLayout(WorldState world, int frame)
    {
        var layout = new Layout("Root");

        if (AnsiConsole.Profile.Width >= 140)
        {
            layout.SplitColumns(
                new Layout("Map").Ratio(1),
                new Layout("Info").Ratio(1));
        }
        else
        {
            layout.SplitRows(
                new Layout("Map").Size(_map.Height + 2),
                new Layout("Info"));
        }

        layout["Map"].Update(BuildMapPanel(world, frame));
        layout["Info"].Update(BuildCompactInfoPanel(world, frame));

        return layout;
    }

    private Panel BuildMapPanel(WorldState world, int frame)
    {
        var entitySymbols = ReadEntitySymbols(world);
        var rows = new List<IRenderable>();

        for (var y = 0; y < _map.Height; y++)
        {
            var line = new List<string>();
            for (var x = 0; x < _map.Width; x++)
            {
                line.Add(entitySymbols.TryGetValue((x, y), out var symbol)
                    ? symbol
                    : RenderTerrain(_map.GetTerrain(x, y)));
            }

            rows.Add(new Markup(string.Concat(line)));
        }

        return new Panel(new Rows(rows.ToArray()))
            .Header($"[bold]World Engine Sandbox[/] [grey]frame {frame}[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green);
    }

    private Panel BuildCompactInfoPanel(WorldState world, int frame)
    {
        var stats = _map.GetTemperatureStats(world.Time.TickCount);
        var table = new Table()
            .NoBorder()
            .AddColumn("[grey]Item[/]")
            .AddColumn("[grey]State[/]");

        table.AddRow("Frame/Tick", $"{frame.ToString(CultureInfo.InvariantCulture)} / {world.Time.TickCount.ToString(CultureInfo.InvariantCulture)}");
        table.AddRow("Map temp", $"{FormatTemperature(stats.Average)} avg | {FormatTemperature(stats.Minimum)} min | {FormatTemperature(stats.Maximum)} max");
        table.AddRow("Ambient", FormatTemperature(ReadEngineAmbient(world)));
        table.AddRow("Terrain", "[green]prairie[/] [darkgreen]forest[/] [blue]water[/] [grey]mountain[/]");
        table.AddRow("Keys", "B=body, C=composite, H=held/action, K=impact, Blood=blood, I=injury");
        table.AddRow("[bold]Entities[/]", string.Empty);

        foreach (var entity in ReadDisplayedEntities(world)
                     .OrderByDescending(entity => entity.HasComponent<CompositeComponent>())
                     .ThenByDescending(entity => entity.HasComponent<HandledImpactComponent>())
                     .ThenByDescending(entity => entity.HasComponent<ImpactResistanceComponent>())
                     .ThenBy(ReadEntityName, StringComparer.Ordinal))
        {
            var name = entity.TryGetComponent<NameComponent>(out var nameComponent) && nameComponent is not null
                ? nameComponent.Name
                : entity.Id.ToString();
            var glyph = entity.TryGetComponent<DisplayGlyphComponent>(out var display) && display is not null
                ? Markup.Escape(display.Glyph)
                : "?";
            var position = entity.TryGetComponent<Position2DComponent>(out var positionComponent) && positionComponent is not null
                ? $"({positionComponent.X},{positionComponent.Y})"
                : "n/a";
            var mass = FormatDisplayedMass(entity);
            var compositeState = entity.TryGetComponent<CompositeIntegrityComponent>(out var compositeIntegrity) && compositeIntegrity is not null
                ? FormatCompositeIntegrity(compositeIntegrity)
                : "[grey]-[/]";
            var compositeParts = entity.TryGetComponent<CompositeComponent>(out var compositeComponent) && compositeComponent is not null
                ? $"{compositeComponent.Parts.Count.ToString(CultureInfo.InvariantCulture)}p"
                : string.Empty;
            var composite = string.IsNullOrWhiteSpace(compositeParts)
                ? compositeState
                : $"{compositeState}/{compositeParts}";
            var temperature = entity.TryGetComponent<TemperatureComponent>(out var temperatureComponent) && temperatureComponent is not null
                ? FormatTemperatureShort(temperatureComponent.Celsius)
                : "pending";
            var comfort = entity.TryGetComponent<ThermalComfortComponent>(out var comfortComponent) && comfortComponent is not null
                ? FormatComfortShort(comfortComponent)
                : "[grey]nf[/]";
            var body = entity.TryGetComponent<BodyIntegrityComponent>(out var bodyIntegrity) && bodyIntegrity is not null
                ? FormatBodyIntegrityShort(bodyIntegrity)
                : "[grey]-[/]";
            var blood = entity.TryGetComponent<BloodComponent>(out var bloodComponent) && bloodComponent is not null
                ? FormatBlood(bloodComponent)
                : "[grey]-[/]";
            var vital = entity.TryGetComponent<VitalStateComponent>(out var vitalComponent) && vitalComponent is not null
                ? FormatVitalState(vitalComponent)
                : string.Empty;
            var injuries = entity.TryGetComponent<InjuryComponent>(out var injuryComponent) && injuryComponent is not null
                ? FormatInjuries(world, injuryComponent)
                : "[grey]-[/]";
            var impact = entity.TryGetComponent<ImpactResultComponent>(out var impactResult) && impactResult is not null
                ? FormatImpact(impactResult)
                : "[grey]-[/]";
            var handling = FormatHandling(entity);

            table.AddRow(
                $"{glyph} {Markup.Escape(name)}",
                $"{position} {mass} {temperature} {comfort} {vital} B{body} C{composite} H{handling} K{impact} Blood{blood} I{injuries}");
        }

        return new Panel(table)
            .Header("[bold]World state[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Yellow);
    }

    private Panel BuildInfoPanel(WorldState world, int frame)
    {
        var stats = _map.GetTemperatureStats(world.Time.TickCount);
        var table = new Table()
            .NoBorder()
            .AddColumn("[grey]Metric[/]")
            .AddColumn("[grey]Value[/]");

        table.AddRow("Frame", frame.ToString(CultureInfo.InvariantCulture));
        table.AddRow("Tick", world.Time.TickCount.ToString(CultureInfo.InvariantCulture));
        table.AddRow("Map temp", $"{FormatTemperature(stats.Average)} avg | {FormatTemperature(stats.Minimum)} min | {FormatTemperature(stats.Maximum)} max");
        table.AddRow("Ambient", FormatTemperature(ReadEngineAmbient(world)));
        table.AddRow("Entities", world.EntitiesWith<Position2DComponent>().Count.ToString(CultureInfo.InvariantCulture));
        table.AddEmptyRow();
        table.AddRow("[bold]Legend[/]", string.Empty);
        table.AddRow("Prairie", "[green]🌾[/]");
        table.AddRow("Forest", "[darkgreen]🌳[/]");
        table.AddRow("Water", "[blue]🌊[/]");
        table.AddRow("Mountain", "[grey]⛰️[/]");
        table.AddEmptyRow();
        table.AddRow("[bold]Entities[/]", string.Empty);

        foreach (var entity in world.EntitiesWith<Position2DComponent>().OrderBy(ReadEntityName, StringComparer.Ordinal))
        {
            var name = entity.TryGetComponent<NameComponent>(out var nameComponent) && nameComponent is not null
                ? nameComponent.Name
                : entity.Id.ToString();
            var glyph = entity.TryGetComponent<DisplayGlyphComponent>(out var display) && display is not null
                ? Markup.Escape(display.Glyph)
                : "?";
            var position = entity.TryGetComponent<Position2DComponent>(out var positionComponent) && positionComponent is not null
                ? $"({positionComponent.X},{positionComponent.Y})"
                : "n/a";
            var mass = entity.TryGetComponent<MaterialMassComponent>(out var massComponent) && massComponent is not null
                ? $"{massComponent.Kilograms.ToString("0.###", CultureInfo.InvariantCulture)} kg"
                : "pending";
            var temperature = entity.TryGetComponent<TemperatureComponent>(out var temperatureComponent) && temperatureComponent is not null
                ? FormatTemperature(temperatureComponent.Celsius)
                : "n/a";
            var comfort = entity.TryGetComponent<ThermalComfortComponent>(out var comfortComponent) && comfortComponent is not null
                ? FormatComfort(comfortComponent)
                : "[grey]not sensitive[/]";
            var body = entity.TryGetComponent<BodyIntegrityComponent>(out var bodyIntegrity) && bodyIntegrity is not null
                ? FormatBodyIntegrity(bodyIntegrity)
                : "[grey]no body[/]";
            var injuries = entity.TryGetComponent<InjuryComponent>(out var injuryComponent) && injuryComponent is not null
                ? FormatInjuries(world, injuryComponent)
                : "[grey]no injuries[/]";

            table.AddRow(glyph, $"{Markup.Escape(name)} {position} | {mass} | {temperature} | {comfort} | {body} | {injuries}");
        }

        return new Panel(table)
            .Header("[bold]Realtime info[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Yellow);
    }

    private Dictionary<(int X, int Y), string> ReadEntitySymbols(WorldState world)
    {
        var symbols = new Dictionary<(int X, int Y), string>();

        foreach (var entity in world.EntitiesWith<Position2DComponent, DisplayGlyphComponent>())
        {
            var position = entity.GetComponent<Position2DComponent>();
            var glyph = entity.GetComponent<DisplayGlyphComponent>();

            if (position.X < 0 || position.X >= _map.Width || position.Y < 0 || position.Y >= _map.Height)
            {
                continue;
            }

            symbols[(position.X, position.Y)] = $"[bold white]{Markup.Escape(glyph.Glyph)}[/]";
        }

        return symbols;
    }

    private static IReadOnlyCollection<Core.Entity> ReadDisplayedEntities(WorldState world)
    {
        return world.Entities
            .Where(entity => entity.HasComponent<Position2DComponent>() ||
                             entity.HasComponent<CompositeComponent>() ||
                             entity.HasComponent<HeldEntitiesComponent>() ||
                             entity.HasComponent<HandledImpactComponent>())
            .ToArray();
    }

    private static string RenderTerrain(TerrainKind terrain)
    {
        return terrain switch
        {
            TerrainKind.Prairie => "[green]🌾[/]",
            TerrainKind.Forest => "[darkgreen]🌳[/]",
            TerrainKind.Water => "[blue]🌊[/]",
            TerrainKind.Mountain => "[grey]⛰️[/]",
            _ => "[grey].[/]"
        };
    }

    private static string FormatTemperature(double value)
    {
        var text = $"{value.ToString("0.0", CultureInfo.InvariantCulture)} C";
        return value switch
        {
            < 14 => $"[blue]{text}[/]",
            > 23 => $"[red]{text}[/]",
            _ => $"[green]{text}[/]"
        };
    }

    private static string FormatMass(MaterialMassComponent mass)
    {
        return $"{mass.Kilograms.ToString("0.#", CultureInfo.InvariantCulture)}kg";
    }

    private static string FormatDisplayedMass(Core.Entity entity)
    {
        if (entity.TryGetComponent<CompositeMassComponent>(out var compositeMass) && compositeMass is not null)
        {
            return $"{compositeMass.Kilograms.ToString("0.#", CultureInfo.InvariantCulture)}kg";
        }

        return entity.TryGetComponent<MaterialMassComponent>(out var materialMass) && materialMass is not null
            ? FormatMass(materialMass)
            : "pending";
    }

    private static string FormatCompositeIntegrity(CompositeIntegrityComponent integrity)
    {
        var percent = integrity.NormalizedIntegrity * 100;
        var text = integrity.IsStructurallyComplete
            ? $"{percent.ToString("0", CultureInfo.InvariantCulture)}%"
            : $"{percent.ToString("0", CultureInfo.InvariantCulture)}% missing:{Markup.Escape(string.Join(",", integrity.MissingStructuralRoles))}";

        return percent switch
        {
            < 35 => $"[red]{text}[/]",
            < 75 => $"[yellow]{text}[/]",
            _ => $"[green]{text}[/]"
        };
    }

    private static string FormatTemperatureShort(double value)
    {
        var text = $"{value.ToString("0.0", CultureInfo.InvariantCulture)}C";
        return value switch
        {
            < 14 => $"[blue]{text}[/]",
            > 23 => $"[red]{text}[/]",
            _ => $"[green]{text}[/]"
        };
    }

    private static double ReadEngineAmbient(WorldState world)
    {
        var ambientEntity = world.EntitiesWith<AmbientTemperatureComponent>().FirstOrDefault();
        return ambientEntity?.GetComponent<AmbientTemperatureComponent>().Celsius ?? 0;
    }

    private static string FormatComfort(ThermalComfortComponent comfort)
    {
        if (comfort.ColdStress > 0)
        {
            return $"[blue]cold {comfort.ColdStress.ToString("0.0", CultureInfo.InvariantCulture)}[/]";
        }

        if (comfort.HeatStress > 0)
        {
            return $"[red]hot {comfort.HeatStress.ToString("0.0", CultureInfo.InvariantCulture)}[/]";
        }

        return $"[green]comfort {(comfort.Comfort * 100).ToString("0", CultureInfo.InvariantCulture)}%[/]";
    }

    private static string FormatBodyIntegrity(BodyIntegrityComponent integrity)
    {
        var percent = integrity.NormalizedIntegrity * 100;
        var text = $"body {percent.ToString("0", CultureInfo.InvariantCulture)}%";

        return percent switch
        {
            < 35 => $"[red]{text}[/]",
            < 75 => $"[yellow]{text}[/]",
            _ => $"[green]{text}[/]"
        };
    }

    private static string FormatComfortShort(ThermalComfortComponent comfort)
    {
        if (comfort.ColdStress > 0)
        {
            return $"[blue]cold{comfort.ColdStress.ToString("0.0", CultureInfo.InvariantCulture)}[/]";
        }

        if (comfort.HeatStress > 0)
        {
            return $"[red]hot{comfort.HeatStress.ToString("0.0", CultureInfo.InvariantCulture)}[/]";
        }

        return $"[green]ok{(comfort.Comfort * 100).ToString("0", CultureInfo.InvariantCulture)}%[/]";
    }

    private static string FormatBodyIntegrityShort(BodyIntegrityComponent integrity)
    {
        var percent = integrity.NormalizedIntegrity * 100;
        var text = $"{percent.ToString("0", CultureInfo.InvariantCulture)}%";

        return percent switch
        {
            < 35 => $"[red]{text}[/]",
            < 75 => $"[yellow]{text}[/]",
            _ => $"[green]{text}[/]"
        };
    }

    private static string FormatBlood(BloodComponent blood)
    {
        var percent = blood.Ratio * 100;
        var text = $"{percent.ToString("0.0", CultureInfo.InvariantCulture)}%";

        return percent switch
        {
            <= 15 => $"[red]{text}[/]",
            < 45 => $"[yellow]{text}[/]",
            _ => $"[green]{text}[/]"
        };
    }

    private static string FormatVitalState(VitalStateComponent vital)
    {
        if (vital.IsAlive)
        {
            return string.Empty;
        }

        var reason = vital.DeathReason ?? "unknown";
        var label = reason.StartsWith("vital_part_destroyed:structure:", StringComparison.Ordinal)
            ? "broken"
            : "dead";

        return $"[red]{label}:{Markup.Escape(reason)}[/]";
    }

    private static string FormatImpact(ImpactResultComponent impact)
    {
        var text = $"{impact.Outcome}:{impact.ImpactRatio.ToString("0.00", CultureInfo.InvariantCulture)}";

        return impact.Outcome switch
        {
            ImpactOutcome.None => $"[grey]{text}[/]",
            ImpactOutcome.Scratch or ImpactOutcome.Dent => $"[green]{text}[/]",
            ImpactOutcome.Crack or ImpactOutcome.Cut => $"[yellow]{text}[/]",
            _ => $"[red]{text}[/]"
        };
    }

    private static string FormatHandling(Core.Entity entity)
    {
        var heldCount = entity.TryGetComponent<HeldEntitiesComponent>(out var held) && held is not null
            ? held.Entities.Count
            : 0;

        var action = entity.TryGetComponent<HandledImpactComponent>(out var handledImpact) && handledImpact is not null
            ? $"a{handledImpact.TicksRemaining.ToString(CultureInfo.InvariantCulture)}"
            : string.Empty;

        return heldCount > 0 || !string.IsNullOrWhiteSpace(action)
            ? $"{heldCount.ToString(CultureInfo.InvariantCulture)}{action}"
            : "[grey]-[/]";
    }

    private static string FormatInjuries(WorldState world, InjuryComponent injuries)
    {
        if (injuries.Injuries.Count == 0)
        {
            return "[green]-[/]";
        }

        var descriptions = injuries.Injuries
            .OrderBy(injury => injury.BodyPartId, StringComparer.Ordinal)
            .Select(injury => FormatInjury(world, injury))
            .ToArray();

        return string.Join(", ", descriptions);
    }

    private static string FormatInjury(WorldState world, InjuryState injury)
    {
        var definition = world.Definitions.Get<InjuryDefinition>(injury.Injury.Id);
        var bleeding = injury.BleedingSeverity ?? definition.DefaultBleedingSeverity;
        var severityPercent = Math.Clamp(injury.Severity, 0, 1) * 100;
        var bleedingCode = bleeding switch
        {
            BleedingSeverity.None => "N",
            BleedingSeverity.Minor => "Mi",
            BleedingSeverity.Moderate => "Mo",
            BleedingSeverity.Severe => "Se",
            BleedingSeverity.Massive => "Ma",
            _ => "?"
        };
        var injuryName = definition.Name.Replace(' ', '_');
        var text = $"{Markup.Escape(injuryName)}:{Markup.Escape(injury.BodyPartId)}{severityPercent.ToString("0", CultureInfo.InvariantCulture)}%/{bleedingCode}";

        return injury.Severity switch
        {
            >= 0.7 => $"[red]{text}[/]",
            >= 0.3 => $"[yellow]{text}[/]",
            _ => $"[green]{text}[/]"
        };
    }

    private static string ReadEntityName(Core.Entity entity)
    {
        return entity.TryGetComponent<NameComponent>(out var name) && name is not null
            ? name.Name
            : entity.Id.ToString();
    }
}
