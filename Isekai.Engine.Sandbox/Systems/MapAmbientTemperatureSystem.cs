using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Temperature;
using Isekai.Engine.Sandbox.World;

namespace Isekai.Engine.Sandbox.Systems;

/// <summary>
/// Updates the sandbox ambient temperature component from the current map temperature.
/// </summary>
public sealed class MapAmbientTemperatureSystem : IWorldSystem
{
    private readonly SandboxMap _map;

    /// <summary>
    /// Creates a system that exposes sandbox map temperature to engine temperature systems.
    /// </summary>
    public MapAmbientTemperatureSystem(SandboxMap map)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
    }

    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var ambientEntity = context.World.EntitiesWith<AmbientTemperatureComponent>().FirstOrDefault();
        if (ambientEntity is null)
        {
            return;
        }

        var stats = _map.GetTemperatureStats(context.Time.TickCount);
        ambientEntity.SetComponent(new AmbientTemperatureComponent(stats.Average));
    }
}
