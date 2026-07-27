using Isekai.Engine.Core.System;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Refills finite water source entities when configured.
/// </summary>
public sealed class WaterSourceRefillSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var deltaSeconds = NeedMath.ResolveDeltaSeconds(context.DeltaTime);

        foreach (var entity in context.World.EntitiesWith<WaterSourceComponent>())
        {
            var source = entity.GetComponent<WaterSourceComponent>();
            var maximum = NeedMath.SafeNonNegative(source.MaximumVolumeLiters);
            var current = NeedMath.ClampReserve(source.CurrentVolumeLiters, maximum);
            var refill = NeedMath.SafeNonNegative(source.RefillLitersPerSecond) * deltaSeconds;
            entity.SetComponent(source with { CurrentVolumeLiters = NeedMath.ClampReserve(current + refill, maximum) });
        }
    }
}
