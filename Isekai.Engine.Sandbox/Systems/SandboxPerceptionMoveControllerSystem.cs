using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Movement;
using Isekai.Engine.Modules.Perception;

namespace Isekai.Engine.Sandbox.Systems;

/// <summary>
/// Demonstration-only controller that moves toward the nearest perceived food or water.
/// </summary>
public sealed class SandboxPerceptionMoveControllerSystem : IWorldSystem
{
    private readonly AgentObservationBuilder _observationBuilder;
    private readonly double _desiredDistanceMeters;

    /// <summary>
    /// Creates the demo controller.
    /// </summary>
    public SandboxPerceptionMoveControllerSystem(AgentObservationBuilder observationBuilder, double desiredDistanceMeters = 1)
    {
        _observationBuilder = observationBuilder ?? throw new ArgumentNullException(nameof(observationBuilder));
        _desiredDistanceMeters = desiredDistanceMeters;
    }

    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        foreach (var entity in context.World.EntitiesWith<PerceptionCapabilityComponent, MovementCapabilityComponent>())
        {
            if (entity.HasComponent<MoveIntentComponent>())
            {
                continue;
            }

            var observation = _observationBuilder.BuildObservation(context.World, entity.Id);
            var target = observation.PerceivedEntities
                .Where(item => item.Tags.Contains("water") || item.Tags.Contains("food"))
                .OrderBy(item => item.DistanceMeters)
                .ThenBy(item => item.EntityId.Value)
                .FirstOrDefault();

            if (target is null)
            {
                continue;
            }

            entity.AddComponent(new MoveIntentComponent(new MoveIntent(
                entity.Id,
                target.Direction,
                Math.Min(_desiredDistanceMeters, target.DistanceMeters),
                MovementMode.Walk)));
        }
    }
}
