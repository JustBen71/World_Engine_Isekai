using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Injuries;

namespace Isekai.Engine.Modules.Healing;

/// <summary>
/// Applies active treatment from healing-capable entities to targeted injured entities.
/// </summary>
public sealed class TreatmentSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var deltaSeconds = Math.Max(0, context.DeltaTime.TotalSeconds);
        foreach (var healer in context.World.EntitiesWith<HealingCapabilityComponent, HealingTargetComponent>())
        {
            var capability = healer.GetComponent<HealingCapabilityComponent>();
            var target = healer.GetComponent<HealingTargetComponent>();
            if (!context.World.TryGetEntity(target.TargetEntityId, out var targetEntity) ||
                targetEntity is null ||
                !targetEntity.TryGetComponent<InjuryComponent>(out var injuries) ||
                injuries is null)
            {
                continue;
            }

            var treated = injuries.Injuries
                .Where(injury => target.BodyPartId is null || string.Equals(injury.BodyPartId, target.BodyPartId, StringComparison.Ordinal))
                .ToHashSet();

            targetEntity.SetComponent(new InjuryComponent(injuries.Injuries
                .Select(injury => treated.Contains(injury)
                    ? Treat(context, injury, capability, deltaSeconds)
                    : injury)
                .Where(injury => injury.Severity > 0)
                .ToArray()));
        }
    }

    private static InjuryState Treat(
        WorldSystemExecutionContext context,
        InjuryState injury,
        HealingCapabilityComponent capability,
        double deltaSeconds)
    {
        var recovery = capability.TissueTreatmentPerSecond;
        var definition = context.World.Definitions.Get<InjuryDefinition>(injury.Injury.Id);
        var bleeding = injury.BleedingSeverity ?? definition.DefaultBleedingSeverity;
        if (bleeding != BleedingSeverity.None)
        {
            recovery += capability.BleedingTreatmentPerSecond;
        }

        var nextSeverity = Math.Max(0, injury.Severity - recovery * deltaSeconds);
        return injury with
        {
            Severity = nextSeverity,
            IsTreated = true
        };
    }
}
