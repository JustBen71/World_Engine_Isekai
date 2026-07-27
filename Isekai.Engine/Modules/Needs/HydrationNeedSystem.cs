using Isekai.Engine.Core.System;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Decreases hydration reserves passively over simulation time.
/// </summary>
public sealed class HydrationNeedSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var deltaSeconds = NeedMath.ResolveDeltaSeconds(context.DeltaTime);

        foreach (var entity in context.World.EntitiesWith<HydrationNeedComponent>())
        {
            var need = entity.GetComponent<HydrationNeedComponent>();
            var maximum = NeedMath.SafeNonNegative(need.MaximumHydration);
            var consumption = NeedMath.SafeNonNegative(need.PassiveConsumptionPerSecond) * deltaSeconds;
            var current = NeedMath.ClampReserve(need.CurrentHydration, maximum);
            entity.SetComponent(need with { CurrentHydration = NeedMath.ClampReserve(current - consumption, maximum) });
        }
    }
}
