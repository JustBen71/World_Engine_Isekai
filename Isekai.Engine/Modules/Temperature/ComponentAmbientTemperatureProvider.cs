using Isekai.Engine.Core;
using Isekai.Engine.Interfaces;

namespace Isekai.Engine.Modules.Temperature;

/// <summary>
/// Reads ambient temperature from the existing ambient temperature component.
/// </summary>
public sealed class ComponentAmbientTemperatureProvider : IAmbientTemperatureProvider
{
    private readonly double _fallbackAmbientTemperatureCelsius;

    /// <summary>
    /// Creates a provider that reads ambient temperature from world components.
    /// </summary>
    public ComponentAmbientTemperatureProvider(double fallbackAmbientTemperatureCelsius = 20)
    {
        _fallbackAmbientTemperatureCelsius = fallbackAmbientTemperatureCelsius;
    }

    /// <inheritdoc />
    public double GetAmbientTemperatureCelsius(IWorldState world, Entity entity)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(entity);

        var ambientEntity = world.EntitiesWith<AmbientTemperatureComponent>().FirstOrDefault();
        var ambientTemperatureCelsius = ambientEntity?.GetComponent<AmbientTemperatureComponent>().Celsius ??
                                        _fallbackAmbientTemperatureCelsius;

        return double.IsFinite(ambientTemperatureCelsius)
            ? ambientTemperatureCelsius
            : _fallbackAmbientTemperatureCelsius;
    }
}
