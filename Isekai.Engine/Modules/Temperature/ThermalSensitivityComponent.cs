using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Temperature;

/// <summary>
/// Marks an entity as able to feel thermal comfort or stress.
/// </summary>
public sealed record ThermalSensitivityComponent(
    double ComfortableMinimumCelsius,
    double ComfortableMaximumCelsius) : IComponent
{
    /// <summary>
    /// Gets the minimum comfortable temperature in degrees Celsius.
    /// </summary>
    public double MinimumComfortableTemperatureCelsius => ComfortableMinimumCelsius;

    /// <summary>
    /// Gets the maximum comfortable temperature in degrees Celsius.
    /// </summary>
    public double MaximumComfortableTemperatureCelsius => ComfortableMaximumCelsius;
}
