using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Body;

namespace Isekai.Engine.Modules.Impact;

/// <summary>
/// Applies wear to the entity used as the contact surface during an impact.
/// </summary>
public sealed class ImpactContactWearSystem : IWorldSystem
{
    private const double WearScale = 0.01;

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

            if (!context.World.TryGetEntity(result.ContactEntityId, out var contactEntity) ||
                contactEntity is null ||
                !contactEntity.TryGetComponent<ContactSurfaceComponent>(out var surface) ||
                surface is null ||
                !contactEntity.TryGetComponent<BodyStateComponent>(out var body) ||
                body is null)
            {
                continue;
            }

            var damage = CalculateWear(result, surface);
            var targetPartId = body.Parts.FirstOrDefault()?.PartId;
            if (targetPartId is null)
            {
                continue;
            }

            var nextBody = ApplyWear(body, targetPartId, damage);
            contactEntity.SetComponent(nextBody);
            contactEntity.SetComponent(new BodyIntegrityComponent(CalculateIntegrity(nextBody)));
        }
    }

    private static double CalculateWear(
        ImpactResultComponent result,
        ContactSurfaceComponent surface)
    {
        var retention = Math.Max(0.1, surface.EdgeRetention);
        var outcomeMultiplier = result.Outcome switch
        {
            ImpactOutcome.Scratch => 0.05,
            ImpactOutcome.Dent => 0.1,
            ImpactOutcome.Crack => 0.2,
            ImpactOutcome.Cut => 0.35,
            ImpactOutcome.Puncture => 0.45,
            ImpactOutcome.Fracture => 0.65,
            _ => 0
        };

        return Math.Max(0, result.Force) *
               Math.Max(0, result.ImpactRatio) *
               outcomeMultiplier *
               WearScale /
               retention;
    }

    private static BodyStateComponent ApplyWear(
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
