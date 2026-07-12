using Isekai.Engine.Core;
using Isekai.Engine.Core.System;
using Isekai.Engine.Interfaces;
using Isekai.Engine.Modules.Body;

namespace Isekai.Engine.Modules.Vitals;

/// <summary>
/// Determines whether living entities remain alive from blood and vital body parts.
/// </summary>
public sealed class VitalStateSystem : IWorldSystem
{
    private const double FatalBloodRatio = 0.15;

    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entity in context.World.Entities)
        {
            if (!ShouldEvaluate(entity))
            {
                continue;
            }

            var existingState = entity.TryGetComponent<VitalStateComponent>(out var state) ? state : null;
            if (existingState is { IsAlive: false })
            {
                continue;
            }

            var deathReason = FindDeathReason(context.World, entity);
            entity.SetComponent(deathReason is null
                ? new VitalStateComponent(IsAlive: true)
                : new VitalStateComponent(IsAlive: false, deathReason));
        }
    }

    private static bool ShouldEvaluate(Entity entity)
    {
        return entity.HasComponent<BloodComponent>() ||
               entity.HasComponent<VitalStateComponent>() ||
               entity.HasComponent<BodyStateComponent>();
    }

    private static string? FindDeathReason(IWorldState world, Entity entity)
    {
        if (entity.TryGetComponent<BloodComponent>(out var blood) &&
            blood is not null &&
            blood.Ratio <= FatalBloodRatio)
        {
            return "blood_loss";
        }

        if (!entity.TryGetComponent<BodyStateComponent>(out var bodyState) || bodyState is null)
        {
            return null;
        }

        var bodyDefinition = world.Definitions.Get<BodyDefinition>(bodyState.Body.Id);
        var partDefinitions = bodyDefinition.Parts.ToDictionary(part => part.Id, StringComparer.Ordinal);

        foreach (var partState in bodyState.Parts)
        {
            if (!partDefinitions.TryGetValue(partState.PartId, out var definition))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(definition.VitalRole))
            {
                continue;
            }

            if (partState.Integrity <= 0)
            {
                return $"vital_part_destroyed:{definition.VitalRole}:{partState.PartId}";
            }
        }

        return null;
    }
}
