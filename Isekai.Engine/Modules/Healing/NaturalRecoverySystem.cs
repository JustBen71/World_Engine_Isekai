using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Injuries;

namespace Isekai.Engine.Modules.Healing;

/// <summary>
/// Applies entity-specific natural recovery to existing injuries.
/// </summary>
public sealed class NaturalRecoverySystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var deltaSeconds = Math.Max(0, context.DeltaTime.TotalSeconds);
        foreach (var entity in context.World.EntitiesWith<InjuryComponent, NaturalRecoveryComponent>())
        {
            var injuries = entity.GetComponent<InjuryComponent>();
            var recovery = entity.GetComponent<NaturalRecoveryComponent>();
            entity.SetComponent(new InjuryComponent(injuries.Injuries
                .Select(injury => Recover(context, injury, recovery, deltaSeconds))
                .Where(injury => injury.Severity > 0)
                .ToArray()));
        }
    }

    private static InjuryState Recover(
        WorldSystemExecutionContext context,
        InjuryState injury,
        NaturalRecoveryComponent recovery,
        double deltaSeconds)
    {
        var recoveryRate = recovery.TissueRecoveryPerSecond;
        if (ResolvesBleedingNaturally(context, injury))
        {
            recoveryRate += recovery.BleedingRecoveryPerSecond;
        }

        var nextSeverity = Math.Max(0, injury.Severity - recoveryRate * deltaSeconds);
        return injury with { Severity = nextSeverity };
    }

    private static bool ResolvesBleedingNaturally(WorldSystemExecutionContext context, InjuryState injury)
    {
        var definition = context.World.Definitions.Get<InjuryDefinition>(injury.Injury.Id);
        var bleeding = injury.BleedingSeverity ?? definition.DefaultBleedingSeverity;
        return bleeding is BleedingSeverity.None or BleedingSeverity.Minor;
    }
}
