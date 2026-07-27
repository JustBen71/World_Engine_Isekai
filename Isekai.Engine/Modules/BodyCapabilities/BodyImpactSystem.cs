using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Body;
using Isekai.Engine.Modules.Impact;

namespace Isekai.Engine.Modules.BodyCapabilities;

/// <summary>
/// Converts authorized body contact actions into raw impact requests.
/// </summary>
public sealed class BodyImpactSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var actor in context.World.EntitiesWith<BodyImpactCapabilityComponent, BodyContactSurfacesComponent>())
        {
            if (!actor.TryGetComponent<BodyImpactComponent>(out var action) || action is null)
            {
                CleanupStaleImpact(actor);
                continue;
            }

            if (action.TicksRemaining <= 0)
            {
                CleanupStaleImpact(actor);
                actor.RemoveComponent<BodyImpactComponent>();
                continue;
            }

            var surface = actor.GetComponent<BodyContactSurfacesComponent>()
                .Surfaces
                .FirstOrDefault(candidate => string.Equals(candidate.Role, action.ContactRole, StringComparison.Ordinal));
            if (surface is null || !CanUseBodyPart(actor, surface.BodyPartId))
            {
                CleanupStaleImpact(actor);
                continue;
            }

            var capability = actor.GetComponent<BodyImpactCapabilityComponent>();
            actor.SetComponent(AdvanceAction(context, actor, capability, surface, action));
        }
    }

    private static BodyImpactComponent AdvanceAction(
        WorldSystemExecutionContext context,
        Core.Entity actor,
        BodyImpactCapabilityComponent capability,
        BodyContactSurface surface,
        BodyImpactComponent action)
    {
        var ticksBetweenImpacts = Math.Max(1, action.TicksBetweenImpacts);
        var ticksRemaining = Math.Max(0, action.TicksRemaining - 1);

        if (action.TicksUntilNextImpact > 0)
        {
            CleanupStaleImpact(actor);
            return action with
            {
                TicksRemaining = ticksRemaining,
                TicksUntilNextImpact = action.TicksUntilNextImpact - 1
            };
        }

        if (!context.World.ContainsEntity(action.TargetEntityId))
        {
            CleanupStaleImpact(actor);
            return action with
            {
                TicksRemaining = ticksRemaining,
                TicksUntilNextImpact = ticksBetweenImpacts - 1
            };
        }

        actor.SetComponent(new ContactSurfaceComponent(
            surface.Hardness,
            surface.Sharpness,
            surface.Penetration,
            surface.EdgeRetention,
            surface.ContactArea));
        actor.SetComponent(new ImpactRequestComponent(
            actor.Id,
            action.TargetEntityId,
            CalculateForce(capability, action),
            action.TargetBodyPartId));

        return action with
        {
            TicksRemaining = ticksRemaining,
            TicksUntilNextImpact = ticksBetweenImpacts - 1
        };
    }

    private static bool CanUseBodyPart(Core.Entity actor, string bodyPartId)
    {
        if (!actor.TryGetComponent<BodyStateComponent>(out var body) || body is null)
        {
            return true;
        }

        return body.Parts.Any(part =>
            string.Equals(part.PartId, bodyPartId, StringComparison.Ordinal) &&
            part.Integrity > 0);
    }

    private static double CalculateForce(
        BodyImpactCapabilityComponent capability,
        BodyImpactComponent action)
    {
        var effort = Math.Max(0, action.Effort);
        var precision = Math.Clamp(capability.Precision, 0, 1);
        var force = Math.Max(0, capability.ImpactForce) * effort * precision;

        return Math.Min(Math.Max(0, capability.MaxForce), force);
    }

    private static void CleanupStaleImpact(Core.Entity actor)
    {
        actor.RemoveComponent<ImpactRequestComponent>();
        actor.RemoveComponent<ImpactResultComponent>();
        actor.RemoveComponent<ContactSurfaceComponent>();
    }
}
