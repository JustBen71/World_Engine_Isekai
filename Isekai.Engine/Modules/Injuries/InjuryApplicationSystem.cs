using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Body;

namespace Isekai.Engine.Modules.Injuries;

/// <summary>
/// Applies runtime injuries to body part integrity.
/// </summary>
public sealed class InjuryApplicationSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var deltaSeconds = Math.Max(0, context.DeltaTime.TotalSeconds);

        foreach (var entity in context.World.EntitiesWith<BodyStateComponent, InjuryComponent>())
        {
            var body = entity.GetComponent<BodyStateComponent>();
            var injuries = entity.GetComponent<InjuryComponent>();

            var parts = body.Parts.ToDictionary(part => part.PartId, StringComparer.Ordinal);
            var nextInjuries = new List<InjuryState>();

            foreach (var injury in injuries.Injuries)
            {
                if (!parts.TryGetValue(injury.BodyPartId, out var part))
                {
                    nextInjuries.Add(injury);
                    continue;
                }

                var definition = context.World.Definitions.Get<InjuryDefinition>(injury.Injury.Id);
                var damage = Math.Max(0, injury.Severity) *
                             definition.IntegrityLossPerSeverityPerSecond *
                             deltaSeconds;
                var nextIntegrity = Math.Max(0, part.Integrity - damage);
                parts[injury.BodyPartId] = part with { Integrity = nextIntegrity };

                nextInjuries.Add(injury);
            }

            var nextBody = new BodyStateComponent(body.Body, parts.Values.ToArray());
            entity.SetComponent(nextBody);
            entity.SetComponent(new InjuryComponent(nextInjuries));
            entity.SetComponent(new BodyIntegrityComponent(CalculateIntegrity(nextBody)));
        }
    }

    private static double CalculateIntegrity(BodyStateComponent body)
    {
        var max = body.Parts.Sum(part => part.MaxIntegrity);
        var current = body.Parts.Sum(part => Math.Clamp(part.Integrity, 0, part.MaxIntegrity));

        return max <= 0 ? 0 : current / max;
    }
}
