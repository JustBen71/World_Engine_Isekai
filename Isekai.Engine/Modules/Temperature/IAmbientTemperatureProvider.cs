using Isekai.Engine.Core;
using Isekai.Engine.Interfaces;

namespace Isekai.Engine.Modules.Temperature;

/// <summary>
/// Provides the ambient temperature that surrounds one entity.
/// </summary>
public interface IAmbientTemperatureProvider
{
    /// <summary>
    /// Gets the ambient temperature around an entity in degrees Celsius.
    /// </summary>
    double GetAmbientTemperatureCelsius(IWorldState world, Entity entity);
}
