using Isekai.Engine.Core.System;

namespace Isekai.Engine.Modules.Body;

/// <summary>
/// Initializes runtime body state for entities that have a body definition.
/// </summary>
public sealed class BodyInitializationSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entity in context.World.EntitiesWith<BodyComponent>())
        {
            if (entity.HasComponent<BodyStateComponent>())
            {
                continue;
            }

            var body = entity.GetComponent<BodyComponent>();
            var definition = context.World.Definitions.Get<BodyDefinition>(body.Body.Id);
            var parts = definition.Parts
                .Select(part => new BodyPartState(part.Id, part.MaxIntegrity, part.MaxIntegrity))
                .ToArray();

            entity.SetComponent(new BodyStateComponent(body.Body, parts));
            entity.SetComponent(new BodyIntegrityComponent(1));
        }
    }
}
