using Isekai.Engine.Core;
using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Body;

namespace Isekai.Engine.Modules.Composition;

/// <summary>
/// Calculates whether composite entities are still structurally complete.
/// </summary>
public sealed class CompositeIntegritySystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entity in context.World.EntitiesWith<CompositeComponent>())
        {
            entity.SetComponent(CalculateIntegrity(context, entity.GetComponent<CompositeComponent>()));
        }
    }

    private static CompositeIntegrityComponent CalculateIntegrity(
        WorldSystemExecutionContext context,
        CompositeComponent composite)
    {
        var structuralParts = composite.Parts
            .Where(part => part.IsStructural)
            .ToArray();

        if (structuralParts.Length == 0)
        {
            return new CompositeIntegrityComponent(
                NormalizedIntegrity: 1,
                IsStructurallyComplete: true,
                MissingStructuralRoles: Array.Empty<string>(),
                WeakestStructuralPartId: null,
                WeakestStructuralRole: null);
        }

        var missingRoles = new List<string>();
        var integrityValues = new List<(CompositePart Part, double Integrity)>();

        foreach (var part in structuralParts)
        {
            if (!context.World.TryGetEntity(part.EntityId, out var partEntity) || partEntity is null)
            {
                missingRoles.Add(part.Role);
                integrityValues.Add((part, 0));
                continue;
            }

            var integrity = partEntity.TryGetComponent<BodyIntegrityComponent>(out var bodyIntegrity) &&
                            bodyIntegrity is not null
                ? bodyIntegrity.NormalizedIntegrity
                : 1;

            integrityValues.Add((part, Math.Clamp(integrity, 0, 1)));
        }

        var weakest = integrityValues
            .OrderBy(value => value.Integrity)
            .First();

        return new CompositeIntegrityComponent(
            integrityValues.Average(value => value.Integrity),
            missingRoles.Count == 0,
            missingRoles.ToArray(),
            weakest.Part.EntityId,
            weakest.Part.Role);
    }
}
