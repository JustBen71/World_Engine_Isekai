using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Materials;

namespace Isekai.Engine.Modules.Composition;

/// <summary>
/// Calculates composite mass from its own integrated materials and its important parts.
/// </summary>
public sealed class CompositeMassSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entity in context.World.EntitiesWith<CompositeComponent>())
        {
            var composite = entity.GetComponent<CompositeComponent>();
            var totalMass = ReadOwnMass(entity) + ReadPartMass(context, composite);

            entity.SetComponent(new CompositeMassComponent(totalMass));
        }
    }

    private static double ReadOwnMass(Core.Entity entity)
    {
        return entity.TryGetComponent<MaterialMassComponent>(out var mass) && mass is not null
            ? mass.Kilograms
            : 0;
    }

    private static double ReadPartMass(
        WorldSystemExecutionContext context,
        CompositeComponent composite)
    {
        var mass = 0.0;
        foreach (var part in composite.Parts)
        {
            if (!context.World.TryGetEntity(part.EntityId, out var partEntity) || partEntity is null)
            {
                continue;
            }

            if (partEntity.TryGetComponent<MaterialMassComponent>(out var partMass) && partMass is not null)
            {
                mass += partMass.Kilograms;
            }
        }

        return mass;
    }
}
