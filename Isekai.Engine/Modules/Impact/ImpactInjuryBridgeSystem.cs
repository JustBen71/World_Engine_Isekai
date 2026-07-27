using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Injuries;

namespace Isekai.Engine.Modules.Impact;

/// <summary>
/// Converts resolved impacts into configured injuries on the impacted target.
/// </summary>
public sealed class ImpactInjuryBridgeSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var source in context.World.EntitiesWith<ImpactRequestComponent, ImpactResultComponent>())
        {
            var result = source.GetComponent<ImpactResultComponent>();
            if (result.Outcome == ImpactOutcome.None)
            {
                continue;
            }

            if (!context.World.TryGetEntity(result.TargetEntityId, out var target) ||
                target is null ||
                !target.TryGetComponent<ImpactInjuryProfileComponent>(out var profile) ||
                profile is null)
            {
                continue;
            }

            var rule = FindRule(profile, result.Outcome);
            if (rule is null)
            {
                continue;
            }

            var bodyPartId = string.IsNullOrWhiteSpace(result.TargetBodyPartId)
                ? "unknown"
                : result.TargetBodyPartId;
            var severity = CalculateSeverity(rule, result);
            if (severity <= 0)
            {
                continue;
            }

            AddInjury(target, new InjuryState(
                rule.Injury,
                bodyPartId,
                severity,
                rule.BleedingSeverity));
        }
    }

    private static ImpactInjuryRule? FindRule(ImpactInjuryProfileComponent profile, ImpactOutcome outcome)
    {
        return profile.Rules.FirstOrDefault(rule => rule.Outcome == outcome);
    }

    private static double CalculateSeverity(ImpactInjuryRule rule, ImpactResultComponent result)
    {
        var scaledSeverity = rule.MinimumSeverity + Math.Max(0, result.ImpactRatio) * rule.SeverityPerImpactRatio;
        return Math.Clamp(scaledSeverity, 0, 1);
    }

    private static void AddInjury(Core.Entity target, InjuryState injury)
    {
        var current = target.TryGetComponent<InjuryComponent>(out var injuries) && injuries is not null
            ? injuries.Injuries.ToList()
            : new List<InjuryState>();

        var existingIndex = current.FindIndex(existing =>
            existing.Injury.Id == injury.Injury.Id &&
            string.Equals(existing.BodyPartId, injury.BodyPartId, StringComparison.Ordinal) &&
            existing.BleedingSeverity == injury.BleedingSeverity);

        if (existingIndex >= 0)
        {
            var existing = current[existingIndex];
            current[existingIndex] = existing with
            {
                Severity = Math.Max(existing.Severity, injury.Severity),
                IsTreated = existing.IsTreated && injury.IsTreated
            };
        }
        else
        {
            current.Add(injury);
        }

        target.SetComponent(new InjuryComponent(current));
    }
}
