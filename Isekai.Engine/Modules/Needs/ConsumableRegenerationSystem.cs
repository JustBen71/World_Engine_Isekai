using Isekai.Engine.Core.System;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Regenerates finite consumable resource entities when configured.
/// </summary>
public sealed class ConsumableRegenerationSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var deltaSeconds = NeedMath.ResolveDeltaSeconds(context.DeltaTime);

        foreach (var entity in context.World.EntitiesWith<ConsumableResourceComponent>())
        {
            var resource = entity.GetComponent<ConsumableResourceComponent>();
            var maximum = NeedMath.SafeNonNegative(resource.MaximumQuantity);
            var current = NeedMath.ClampReserve(resource.CurrentQuantity, maximum);
            var regeneration = NeedMath.SafeNonNegative(resource.RegenerationPerSecond) * deltaSeconds;
            entity.SetComponent(resource with { CurrentQuantity = NeedMath.ClampReserve(current + regeneration, maximum) });
        }
    }
}
