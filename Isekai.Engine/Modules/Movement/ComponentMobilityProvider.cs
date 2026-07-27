using Isekai.Engine.Core;
using Isekai.Engine.Interfaces;

namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Reads optional mobility multiplier components and falls back to full mobility.
/// </summary>
public sealed class ComponentMobilityProvider : IMobilityProvider
{
    /// <inheritdoc />
    public double GetMobilityMultiplier(IWorldState world, Entity entity)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(entity);

        if (!entity.TryGetComponent<MobilityModifierComponent>(out var modifier) || modifier is null)
        {
            return 1;
        }

        return double.IsFinite(modifier.MobilityMultiplier)
            ? Math.Clamp(modifier.MobilityMultiplier, 0, 1)
            : 0;
    }
}
