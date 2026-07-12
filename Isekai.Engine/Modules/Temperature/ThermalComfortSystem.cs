using Isekai.Engine.Core.System;

namespace Isekai.Engine.Modules.Temperature;

/// <summary>
/// Calculates thermal comfort only for entities that have thermal sensitivity.
/// </summary>
public sealed class ThermalComfortSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entity in context.World.EntitiesWith<TemperatureComponent, ThermalSensitivityComponent>())
        {
            var temperature = entity.GetComponent<TemperatureComponent>();
            var sensitivity = entity.GetComponent<ThermalSensitivityComponent>();

            var coldStress = Math.Max(0, sensitivity.ComfortableMinimumCelsius - temperature.Celsius);
            var heatStress = Math.Max(0, temperature.Celsius - sensitivity.ComfortableMaximumCelsius);
            var stress = coldStress + heatStress;
            var comfort = Math.Clamp(1 - (stress / 20.0), 0, 1);

            entity.SetComponent(new ThermalComfortComponent(comfort, coldStress, heatStress));
        }
    }
}
