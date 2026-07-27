using Isekai.Engine.Core.System;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Decreases energy reserves passively over simulation time.
/// </summary>
public sealed class EnergyNeedSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var deltaSeconds = NeedMath.ResolveDeltaSeconds(context.DeltaTime);

        foreach (var entity in context.World.EntitiesWith<EnergyNeedComponent>())
        {
            var need = entity.GetComponent<EnergyNeedComponent>();
            var maximum = NeedMath.SafeNonNegative(need.MaximumEnergy);
            var consumption = NeedMath.SafeNonNegative(need.PassiveConsumptionPerSecond) * deltaSeconds;
            var current = NeedMath.ClampReserve(need.CurrentEnergy, maximum);
            entity.SetComponent(need with { CurrentEnergy = NeedMath.ClampReserve(current - consumption, maximum) });
        }
    }
}
