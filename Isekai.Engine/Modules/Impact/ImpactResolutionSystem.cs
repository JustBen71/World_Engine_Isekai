using Isekai.Engine.Core.System;

namespace Isekai.Engine.Modules.Impact;

/// <summary>
/// Resolves requested impacts from contact surface coefficients and target resistance coefficients.
/// </summary>
public sealed class ImpactResolutionSystem : IWorldSystem
{
    private const double BaselineResistance = 10_000;
    private const double MinimumContactArea = 0.0001;

    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var source in context.World.EntitiesWith<ImpactRequestComponent>())
        {
            var request = source.GetComponent<ImpactRequestComponent>();
            if (!context.World.TryGetEntity(request.ContactEntityId, out var contactEntity) ||
                contactEntity is null ||
                !context.World.TryGetEntity(request.TargetEntityId, out var targetEntity) ||
                targetEntity is null ||
                !contactEntity.TryGetComponent<ContactSurfaceComponent>(out var surface) ||
                surface is null)
            {
                source.RemoveComponent<ImpactResultComponent>();
                continue;
            }

            var resistance = targetEntity.TryGetComponent<ImpactResistanceComponent>(out var targetResistance) &&
                             targetResistance is not null
                ? targetResistance
                : new ImpactResistanceComponent(1, 1, 1);

            var ratio = CalculateImpactRatio(request, surface, resistance);
            var outcome = ResolveOutcome(ratio, surface);
            var result = new ImpactResultComponent(
                request.ContactEntityId,
                request.TargetEntityId,
                request.TargetBodyPartId,
                request.Force,
                ratio,
                outcome);

            source.SetComponent(result);
            context.EventBus.Publish(new ImpactResolvedEvent(
                source.Id,
                result.ContactEntityId,
                result.TargetEntityId,
                result.TargetBodyPartId,
                result.Force,
                result.ImpactRatio,
                result.Outcome));
        }
    }

    private static double CalculateImpactRatio(
        ImpactRequestComponent request,
        ContactSurfaceComponent surface,
        ImpactResistanceComponent resistance)
    {
        var contactArea = Math.Max(MinimumContactArea, surface.ContactArea);
        var shapeScore = Math.Max(0, surface.Hardness) *
                         (0.5 + Math.Max(0, surface.Sharpness)) *
                         Math.Max(0, surface.Penetration);
        var resistanceScore = BaselineResistance *
                              Math.Max(0.1, resistance.Hardness) *
                              Math.Max(0.1, resistance.Toughness) *
                              Math.Max(0.1, resistance.FractureResistance);

        return Math.Max(0, request.Force) * shapeScore / (contactArea * resistanceScore);
    }

    private static ImpactOutcome ResolveOutcome(double impactRatio, ContactSurfaceComponent surface)
    {
        if (impactRatio < 0.25)
        {
            return ImpactOutcome.None;
        }

        if (impactRatio < 0.5)
        {
            return ImpactOutcome.Scratch;
        }

        if (impactRatio < 0.9)
        {
            return ImpactOutcome.Dent;
        }

        if (impactRatio < 1.3)
        {
            return ImpactOutcome.Crack;
        }

        if (impactRatio < 1.8)
        {
            return surface.Sharpness >= surface.Penetration
                ? ImpactOutcome.Cut
                : ImpactOutcome.Puncture;
        }

        if (impactRatio < 2.5)
        {
            return ImpactOutcome.Puncture;
        }

        return ImpactOutcome.Fracture;
    }
}
