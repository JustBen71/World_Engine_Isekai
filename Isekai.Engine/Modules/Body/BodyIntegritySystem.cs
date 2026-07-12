using Isekai.Engine.Core.System;

namespace Isekai.Engine.Modules.Body;

/// <summary>
/// Calculates aggregate body integrity from body part states.
/// </summary>
public sealed class BodyIntegritySystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entity in context.World.EntitiesWith<BodyStateComponent>())
        {
            var body = entity.GetComponent<BodyStateComponent>();
            var max = body.Parts.Sum(part => part.MaxIntegrity);
            var current = body.Parts.Sum(part => Math.Clamp(part.Integrity, 0, part.MaxIntegrity));
            var integrity = max <= 0 ? 0 : current / max;

            entity.SetComponent(new BodyIntegrityComponent(integrity));
        }
    }
}
