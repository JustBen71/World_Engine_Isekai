using Isekai.Engine.Core;
using Isekai.Engine.Interfaces;
using Isekai.Engine.Modules.Temperature;
using Isekai.Engine.Sandbox.Components;

namespace Isekai.Engine.Sandbox.Systems;

/// <summary>
/// Demonstrates local ambient temperatures without introducing a terrain module.
/// </summary>
public sealed class SandboxZoneAmbientTemperatureProvider : IAmbientTemperatureProvider
{
    private readonly int _mapWidth;
    private readonly double _coldZoneTemperatureCelsius;
    private readonly double _temperateZoneTemperatureCelsius;
    private readonly double _hotZoneTemperatureCelsius;
    private readonly double _fallbackTemperatureCelsius;

    /// <summary>
    /// Creates a provider that splits the sandbox map into cold, temperate and hot vertical zones.
    /// </summary>
    public SandboxZoneAmbientTemperatureProvider(
        int mapWidth,
        double coldZoneTemperatureCelsius = 5,
        double temperateZoneTemperatureCelsius = 20,
        double hotZoneTemperatureCelsius = 35,
        double fallbackTemperatureCelsius = 20)
    {
        _mapWidth = Math.Max(1, mapWidth);
        _coldZoneTemperatureCelsius = coldZoneTemperatureCelsius;
        _temperateZoneTemperatureCelsius = temperateZoneTemperatureCelsius;
        _hotZoneTemperatureCelsius = hotZoneTemperatureCelsius;
        _fallbackTemperatureCelsius = fallbackTemperatureCelsius;
    }

    /// <inheritdoc />
    public double GetAmbientTemperatureCelsius(IWorldState world, Entity entity)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(entity);

        if (!entity.TryGetComponent<Position2DComponent>(out var position) || position is null)
        {
            return _fallbackTemperatureCelsius;
        }

        var clampedX = Math.Clamp(position.X, 0, _mapWidth - 1);
        var zoneWidth = Math.Max(1, _mapWidth / 3);

        if (clampedX < zoneWidth)
        {
            return _coldZoneTemperatureCelsius;
        }

        if (clampedX < zoneWidth * 2)
        {
            return _temperateZoneTemperatureCelsius;
        }

        return _hotZoneTemperatureCelsius;
    }
}
