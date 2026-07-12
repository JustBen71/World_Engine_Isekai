using Isekai.Engine.Core;
using Isekai.Engine.Core.System;

namespace Isekai.Engine.Modules.Composition;

/// <summary>
/// Synchronizes membership components on entities referenced by composites.
/// </summary>
public sealed class CompositeMembershipSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var memberships = ReadMemberships(context);

        foreach (var entity in context.World.Entities)
        {
            if (memberships.TryGetValue(entity.Id, out var membership))
            {
                entity.SetComponent(membership);
            }
            else
            {
                entity.RemoveComponent<CompositeMembershipComponent>();
            }
        }
    }

    private static Dictionary<EntityId, CompositeMembershipComponent> ReadMemberships(
        WorldSystemExecutionContext context)
    {
        var memberships = new Dictionary<EntityId, CompositeMembershipComponent>();

        foreach (var compositeEntity in context.World.EntitiesWith<CompositeComponent>())
        {
            var composite = compositeEntity.GetComponent<CompositeComponent>();
            foreach (var part in composite.Parts)
            {
                if (!context.World.ContainsEntity(part.EntityId))
                {
                    continue;
                }

                memberships[part.EntityId] = new CompositeMembershipComponent(
                    compositeEntity.Id,
                    part.Role,
                    part.IsStructural);
            }
        }

        return memberships;
    }
}
