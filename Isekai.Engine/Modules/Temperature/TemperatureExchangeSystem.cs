using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Materials;

namespace Isekai.Engine.Modules.Temperature;

/// <summary>
/// Moves physical entity temperatures toward ambient temperature.
/// </summary>
public sealed class TemperatureExchangeSystem : IWorldSystem
{
    private readonly double _fallbackAmbientCelsius;

    /// <summary>
    /// Creates a temperature exchange system.
    /// </summary>
    public TemperatureExchangeSystem(double fallbackAmbientCelsius = 20)
    {
        _fallbackAmbientCelsius = fallbackAmbientCelsius;
    }

    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var ambient = ResolveAmbientTemperature(context);
        var deltaSeconds = Math.Max(0, context.DeltaTime.TotalSeconds);

        foreach (var entity in context.World.EntitiesWith<TemperatureComponent>())
        {
            var current = entity.GetComponent<TemperatureComponent>();
            var rate = ResolveExchangeRate(context, entity);
            var factor = Math.Clamp(rate * deltaSeconds, 0, 1);
            var next = current.Celsius + ((ambient - current.Celsius) * factor);

            entity.SetComponent(new TemperatureComponent(next));
        }
    }

    private double ResolveAmbientTemperature(WorldSystemExecutionContext context)
    {
        var ambientEntity = context.World.EntitiesWith<AmbientTemperatureComponent>().FirstOrDefault();
        return ambientEntity?.GetComponent<AmbientTemperatureComponent>().Celsius ?? _fallbackAmbientCelsius;
    }

    private double ResolveExchangeRate(WorldSystemExecutionContext context, Core.Entity entity)
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

        if (heatCapacity <= 0)
        {
            return 0.05;
        }

        return Math.Clamp((conductance / heatCapacity) * 1000, 0.005, 0.2);
    }
}
