using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Materials;

namespace Isekai.Engine.Modules.Temperature;

/// <summary>
/// Moves physical entity temperatures toward ambient temperature.
/// </summary>
public sealed class TemperatureExchangeSystem : IWorldSystem
{
    private readonly IAmbientTemperatureProvider _ambientTemperatureProvider;

    /// <summary>
    /// Creates a temperature exchange system.
    /// </summary>
    public TemperatureExchangeSystem(double fallbackAmbientCelsius = 20)
        : this(new ComponentAmbientTemperatureProvider(fallbackAmbientCelsius))
    {
    }

    /// <summary>
    /// Creates a temperature exchange system with an explicit ambient temperature provider.
    /// </summary>
    public TemperatureExchangeSystem(IAmbientTemperatureProvider ambientTemperatureProvider)
    {
        _ambientTemperatureProvider = ambientTemperatureProvider ??
                                      throw new ArgumentNullException(nameof(ambientTemperatureProvider));
    }

    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var deltaSeconds = ResolveDeltaSeconds(context);

        foreach (var entity in context.World.EntitiesWith<TemperatureComponent>())
        {
            var current = entity.GetComponent<TemperatureComponent>();
            if (!double.IsFinite(current.Celsius))
            {
                continue;
            }

            var ambient = _ambientTemperatureProvider.GetAmbientTemperatureCelsius(context.World, entity);
            if (!double.IsFinite(ambient))
            {
                continue;
            }

            var rate = ResolveExchangeRate(context, entity);
            var next = CalculateNextTemperature(current.Celsius, ambient, rate, deltaSeconds);

            entity.SetComponent(new TemperatureComponent(next));
        }
    }

    private static double ResolveDeltaSeconds(WorldSystemExecutionContext context)
    {
        var deltaSeconds = context.DeltaTime.TotalSeconds;
        return double.IsFinite(deltaSeconds) ? Math.Max(0, deltaSeconds) : 0;
    }

    private static double CalculateNextTemperature(
        double currentTemperatureCelsius,
        double ambientTemperatureCelsius,
        double exchangeRatePerSecond,
        double deltaSeconds)
    {
        if (deltaSeconds <= 0)
        {
            return currentTemperatureCelsius;
        }

        var safeRate = double.IsFinite(exchangeRatePerSecond)
            ? Math.Max(0, exchangeRatePerSecond)
            : 0;
        var factor = Math.Clamp(safeRate * deltaSeconds, 0, 1);
        var next = currentTemperatureCelsius +
                   ((ambientTemperatureCelsius - currentTemperatureCelsius) * factor);

        if (!double.IsFinite(next))
        {
            return currentTemperatureCelsius;
        }

        return ambientTemperatureCelsius >= currentTemperatureCelsius
            ? Math.Clamp(next, currentTemperatureCelsius, ambientTemperatureCelsius)
            : Math.Clamp(next, ambientTemperatureCelsius, currentTemperatureCelsius);
    }

    private static double ResolveExchangeRate(WorldSystemExecutionContext context, Core.Entity entity)
    {
        if (!entity.TryGetComponent<MaterialCompositionComponent>(out var composition) ||
            composition is null ||
            composition.Materials.Count == 0)
        {
            return 0.05;
        }

        var conductance = 0.0;
        var heatCapacity = 0.0;

        foreach (var quantity in composition.Materials)
        {
            var material = context.World.Definitions.Get<MaterialDefinition>(quantity.Material.Id);
            conductance += material.ThermalConductivity * quantity.Volume;
            heatCapacity += material.Density * quantity.Volume * material.SpecificHeatCapacity;
        }

        if (!double.IsFinite(conductance) ||
            !double.IsFinite(heatCapacity) ||
            conductance < 0 ||
            heatCapacity <= 0)
        {
            return 0.05;
        }

        return Math.Clamp((conductance / heatCapacity) * 1000, 0.005, 0.2);
    }
}
