using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Body;
using Isekai.Engine.Modules.Composition;
using Isekai.Engine.Modules.Materials;

namespace Isekai.Engine.Modules.Impact;

/// <summary>
/// Resolves requested impacts from force, source mass, contact surface coefficients and target resistance coefficients.
/// </summary>
public sealed class ImpactResolutionSystem : IWorldSystem
{
    private const double BaselineForce = 115;
    private const double BaselineContactArea = 0.01;
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
                surface is null ||
                !IsContactUsable(contactEntity))
            {
                source.RemoveComponent<ImpactResultComponent>();
                continue;
            }

            var resistance = targetEntity.TryGetComponent<ImpactResistanceComponent>(out var targetResistance) &&
                             targetResistance is not null
                ? targetResistance
                : new ImpactResistanceComponent(1, 1, 1);

            var ratio = CalculateImpactRatio(source, request, surface, resistance);
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
        Core.Entity source,
        ImpactRequestComponent request,
        ContactSurfaceComponent surface,
        ImpactResistanceComponent resistance)
    {
        var contactArea = Math.Max(MinimumContactArea, surface.ContactArea);
        var forceScore = Math.Max(0, request.Force) / BaselineForce;
        var massScore = CalculateMassScore(source);
        var contactFocus = Math.Clamp(Math.Sqrt(BaselineContactArea / contactArea), 0.35, 3);
        var surfaceScore = Math.Max(0, surface.Hardness) *
                           (1 +
                            Math.Max(0, surface.Sharpness) * 0.55 +
                            Math.Max(0, surface.Penetration) * 0.75);
        var resistanceScore = Math.Max(
            0.1,
            Math.Max(0, resistance.Hardness) * 0.35 +
            Math.Max(0, resistance.Toughness) * 0.35 +
            Math.Max(0, resistance.FractureResistance) * 0.30);

        return forceScore * massScore * contactFocus * surfaceScore / resistanceScore;
    }

    private static double CalculateMassScore(Core.Entity source)
    {
        var mass = ReadImpactMass(source);
        if (mass <= 0)
        {
            return 1;
        }

        return Math.Clamp(0.75 + Math.Sqrt(mass) * 0.22, 0.75, 1.9);
    }

    private static double ReadImpactMass(Core.Entity source)
    {
        if (source.TryGetComponent<CompositeMassComponent>(out var compositeMass) && compositeMass is not null)
        {
            return compositeMass.Kilograms;
        }

        if (source.TryGetComponent<MaterialMassComponent>(out var materialMass) && materialMass is not null)
        {
            return materialMass.Kilograms;
        }

        return 0;
    }

    private static bool IsContactUsable(Core.Entity contactEntity)
    {
        if (contactEntity.TryGetComponent<BodyIntegrityComponent>(out var integrity) &&
            integrity is not null &&
            integrity.NormalizedIntegrity <= 0)
        {
            return false;
        }

        if (!contactEntity.TryGetComponent<BodyStateComponent>(out var body) || body is null)
        {
            return true;
        }

        return body.Parts.Any(part => part.Integrity > 0);
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
            return surface.Sharpness >= surface.Penetration
                ? ImpactOutcome.Cut
                : ImpactOutcome.Puncture;
        }

        return ImpactOutcome.Fracture;
    }
}
