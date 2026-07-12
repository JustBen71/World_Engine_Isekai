using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Vitals;

namespace Isekai.Engine.Modules.Injuries;

/// <summary>
/// Converts bleeding injuries into blood loss for entities that have blood.
/// </summary>
public sealed class BleedingSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var deltaSeconds = Math.Max(0, context.DeltaTime.TotalSeconds);
        foreach (var entity in context.World.EntitiesWith<InjuryComponent, BloodComponent>())
        {
            var injuries = entity.GetComponent<InjuryComponent>();
            var blood = entity.GetComponent<BloodComponent>();
            var bloodLoss = injuries.Injuries.Sum(injury => CalculateBloodLoss(context, injury, deltaSeconds));

            if (bloodLoss <= 0)
            {
                continue;
            }

            entity.SetComponent(new BloodComponent(
                blood.CurrentVolumeLiters - bloodLoss,
                blood.MaxVolumeLiters));
        }
    }

    private static double CalculateBloodLoss(
        WorldSystemExecutionContext context,
        InjuryState injury,
        double deltaSeconds)
    {
        if (injury.IsTreated)
        {
            return 0;
        }

        var definition = context.World.Definitions.Get<InjuryDefinition>(injury.Injury.Id);
        var bleeding = injury.BleedingSeverity ?? definition.DefaultBleedingSeverity;
        var litersPerSecond = bleeding switch
        {
            BleedingSeverity.None => 0,
            BleedingSeverity.Minor => 0.001,
            BleedingSeverity.Moderate => 0.015,
            BleedingSeverity.Severe => 0.05,
            BleedingSeverity.Massive => 0.12,
            _ => 0
        };

        return litersPerSecond * Math.Clamp(injury.Severity, 0, 1) * deltaSeconds;
    }
}
