using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Temperature;

/// <summary>
/// Stores the physical temperature of an entity.
/// </summary>
public sealed record TemperatureComponent(double Celsius) : IComponent
{
    /// <summary>
    /// Gets the current physical temperature in degrees Celsius.
    /// </summary>
    public double CurrentTemperatureCelsius => Celsius;
}
