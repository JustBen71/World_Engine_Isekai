using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Body;

namespace Isekai.Engine.Modules.Impact;

/// <summary>
/// Converts resolved impacts into generic body part integrity loss.
/// </summary>
public sealed class ImpactToBodyDamageSystem : IWorldSystem
{
    private const double DamageScale = 0.06;

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
                !target.TryGetComponent<BodyStateComponent>(out var body) ||
                body is null)
            {
                continue;
            }

            var targetPartId = ResolveTargetPartId(result, body);
            if (targetPartId is null)
            {
                continue;
            }

            var damage = CalculateDamage(result);
            var nextBody = ApplyDamage(body, targetPartId, damage);
            target.SetComponent(nextBody);
            target.SetComponent(new BodyIntegrityComponent(CalculateIntegrity(nextBody)));
        }
    }

    private static string? ResolveTargetPartId(ImpactResultComponent result, BodyStateComponent body)
    {
        if (!string.IsNullOrWhiteSpace(result.TargetBodyPartId))
        {
            return result.TargetBodyPartId;
        }

        return body.Parts.FirstOrDefault()?.PartId;
    }

    private static double CalculateDamage(ImpactResultComponent result)
    {
        var outcomeMultiplier = result.Outcome switch
        {
            ImpactOutcome.Scratch => 0.1,
            ImpactOutcome.Dent => 0.2,
            ImpactOutcome.Crack => 0.45,
            ImpactOutcome.Cut => 0.65,
            ImpactOutcome.Puncture => 0.8,
            ImpactOutcome.Fracture => 1.1,
            _ => 0
        };

        return Math.Max(0, result.Force) *
               Math.Max(0, result.ImpactRatio) *
               outcomeMultiplier *
               DamageScale;
    }

    private static BodyStateComponent ApplyDamage(
        BodyStateComponent body,
        string targetPartId,
        double damage)
    {
        var parts = body.Parts
            .Select(part => string.Equals(part.PartId, targetPartId, StringComparison.Ordinal)
                ? part with { Integrity = Math.Max(0, part.Integrity - damage) }
                : part)
            .ToArray();

        return new BodyStateComponent(body.Body, parts);
    }

    private static double CalculateIntegrity(BodyStateComponent body)
    {
        var max = body.Parts.Sum(part => part.MaxIntegrity);
        var current = body.Parts.Sum(part => Math.Clamp(part.Integrity, 0, part.MaxIntegrity));

        return max <= 0 ? 0 : current / max;
    }
}
