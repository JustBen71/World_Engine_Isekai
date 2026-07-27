using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Impact;

namespace Isekai.Engine.Modules.Handling;

/// <summary>
/// Converts held-entity impact actions into raw impact requests at a controlled rhythm.
/// </summary>
public sealed class HandledImpactSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var actor in context.World.EntitiesWith<GripCapabilityComponent, HeldEntitiesComponent>())
        {
            if (!actor.TryGetComponent<HandledImpactComponent>(out var action) || action is null)
            {
                continue;
            }

            if (!context.World.TryGetEntity(action.HeldEntityId, out var heldEntity) ||
                heldEntity is null ||
                action.TicksRemaining <= 0)
            {
                CleanupStaleImpact(heldEntity);
                actor.RemoveComponent<HandledImpactComponent>();
                continue;
            }

            var held = actor.GetComponent<HeldEntitiesComponent>()
                .Entities
                .FirstOrDefault(heldEntityData => heldEntityData.EntityId == action.HeldEntityId);
            if (held is null)
            {
                CleanupStaleImpact(heldEntity);
                continue;
            }

            var grip = actor.GetComponent<GripCapabilityComponent>();
            var nextAction = AdvanceAction(context, heldEntity, held, grip, action);
            actor.SetComponent(nextAction);
        }
    }

    private static HandledImpactComponent AdvanceAction(
        WorldSystemExecutionContext context,
        Core.Entity heldEntity,
        HeldEntity held,
        GripCapabilityComponent grip,
        HandledImpactComponent action)
    {
        var ticksBetweenImpacts = Math.Max(1, action.TicksBetweenImpacts);
        var ticksRemaining = Math.Max(0, action.TicksRemaining - 1);

        if (action.TicksUntilNextImpact > 0)
        {
            CleanupStaleImpact(heldEntity);
            return action with
            {
                TicksRemaining = ticksRemaining,
                TicksUntilNextImpact = action.TicksUntilNextImpact - 1
            };
        }

        if (!context.World.ContainsEntity(action.ContactEntityId) ||
            !context.World.ContainsEntity(action.TargetEntityId))
        {
            CleanupStaleImpact(heldEntity);
            return action with
            {
                TicksRemaining = ticksRemaining,
                TicksUntilNextImpact = ticksBetweenImpacts - 1
            };
        }

        heldEntity.SetComponent(new ImpactRequestComponent(
            action.ContactEntityId,
            action.TargetEntityId,
            CalculateForce(grip, held, action),
            action.TargetBodyPartId));

        return action with
        {
            TicksRemaining = ticksRemaining,
            TicksUntilNextImpact = ticksBetweenImpacts - 1
        };
    }

    private static double CalculateForce(
        GripCapabilityComponent grip,
        HeldEntity held,
        HandledImpactComponent action)
    {
        var effort = Math.Max(0, action.Effort);
        var gripQuality = Math.Clamp(held.GripQuality, 0, 1);
        var precision = Math.Clamp(grip.Precision, 0, 1);
        var force = Math.Max(0, grip.ManipulationForce) * effort * gripQuality * precision;

        return Math.Min(Math.Max(0, grip.MaxGripForce), force);
    }

    private static void CleanupStaleImpact(Core.Entity? heldEntity)
    {
        if (heldEntity is null)
        {
            return;
        }

        heldEntity.RemoveComponent<ImpactRequestComponent>();
        heldEntity.RemoveComponent<ImpactResultComponent>();
    }
}
