using Isekai.Engine.Core;
using Isekai.Engine.Core.System;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Resolves generic food consumption intents.
/// </summary>
public sealed class ConsumeActionSystem : IWorldSystem
{
    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var intentEntity in context.World.EntitiesWith<ConsumeIntentComponent>())
        {
            var intent = intentEntity.GetComponent<ConsumeIntentComponent>().Intent;
            var result = Resolve(context, intent);

            intentEntity.SetComponent(new ConsumeActionResultComponent(result));
            intentEntity.RemoveComponent<ConsumeIntentComponent>();
        }
    }

    private static ConsumeActionResult Resolve(WorldSystemExecutionContext context, ConsumeIntent intent)
    {
        if (!NeedMath.IsPositiveFinite(intent.RequestedQuantity))
        {
            return EmptyResult(intent, ConsumeOutcome.InvalidQuantity);
        }

        if (!context.World.TryGetEntity(intent.ConsumerEntityId, out var consumer) || consumer is null)
        {
            return EmptyResult(intent, ConsumeOutcome.MissingConsumer);
        }

        if (!context.World.TryGetEntity(intent.ResourceEntityId, out var resourceEntity) || resourceEntity is null)
        {
            return EmptyResult(intent, ConsumeOutcome.MissingResource);
        }

        if (!NeedsSpatial.IsWithinRange(consumer, resourceEntity, intent.MaximumDistanceMeters))
        {
            return EmptyResult(intent, ConsumeOutcome.OutOfRange);
        }

        if (!resourceEntity.TryGetComponent<ConsumableResourceComponent>(out var resource) || resource is null)
        {
            return EmptyResult(intent, ConsumeOutcome.MissingResource);
        }

        var available = NeedMath.ClampReserve(resource.CurrentQuantity, resource.MaximumQuantity);
        if (available <= 0)
        {
            return EmptyResult(intent, ConsumeOutcome.ResourceEmpty);
        }

        if (!consumer.TryGetComponent<DietComponent>(out var diet) || diet is null)
        {
            return EmptyResult(intent, ConsumeOutcome.MissingDiet);
        }

        var definition = context.World.Definitions.Get<ConsumableDefinition>(resource.Definition.Id);
        var digestibility = ResolveDigestibility(definition, diet);
        if (digestibility <= 0)
        {
            return EmptyResult(intent, ConsumeOutcome.NotDigestible);
        }

        if (!consumer.HasComponent<EnergyNeedComponent>() && !consumer.HasComponent<HydrationNeedComponent>())
        {
            return EmptyResult(intent, ConsumeOutcome.MissingNeeds);
        }

        var consumed = Math.Min(available, intent.RequestedQuantity);
        var energyReceived = consumed * definition.EnergyPerUnit * digestibility;
        var hydrationReceived = consumed * definition.HydrationPerUnit * digestibility;
        var toxicityExposure = consumed * definition.ToxicityPerUnit * Math.Max(0, 1 - digestibility);

        ApplyEnergy(consumer, energyReceived);
        ApplyHydration(consumer, hydrationReceived);
        ApplyToxicityExposure(consumer, toxicityExposure, intent.ResourceEntityId, context.Time.TickCount);

        var remaining = Math.Max(0, available - consumed);
        resourceEntity.SetComponent(resource with { CurrentQuantity = remaining });
        if (remaining <= 0 && resource.RemoveEntityWhenEmpty)
        {
            context.World.RemoveEntity(resourceEntity.Id);
        }

        var outcome = consumed < intent.RequestedQuantity ? ConsumeOutcome.PartialSuccess : ConsumeOutcome.Success;
        return new ConsumeActionResult(
            intent.ConsumerEntityId,
            intent.ResourceEntityId,
            intent.RequestedQuantity,
            consumed,
            energyReceived,
            hydrationReceived,
            toxicityExposure,
            outcome);
    }

    private static ConsumeActionResult EmptyResult(ConsumeIntent intent, ConsumeOutcome outcome)
    {
        return new ConsumeActionResult(intent.ConsumerEntityId, intent.ResourceEntityId, intent.RequestedQuantity, 0, 0, 0, 0, outcome);
    }

    private static double ResolveDigestibility(ConsumableDefinition definition, DietComponent diet)
    {
        var best = 0.0;
        var matched = false;
        foreach (var tag in definition.Tags)
        {
            if (diet.TagDigestibility.TryGetValue(tag, out var value) && double.IsFinite(value))
            {
                matched = true;
                best = Math.Max(best, Math.Clamp(value, 0, 1));
            }
        }

        return matched ? best : Math.Clamp(definition.DefaultDigestibility, 0, 1);
    }

    private static void ApplyEnergy(Entity consumer, double energy)
    {
        if (!consumer.TryGetComponent<EnergyNeedComponent>(out var need) || need is null)
        {
            return;
        }

        consumer.SetComponent(need with
        {
            CurrentEnergy = NeedMath.ClampReserve(need.CurrentEnergy + energy, need.MaximumEnergy)
        });
    }

    private static void ApplyHydration(Entity consumer, double hydration)
    {
        if (!consumer.TryGetComponent<HydrationNeedComponent>(out var need) || need is null)
        {
            return;
        }

        consumer.SetComponent(need with
        {
            CurrentHydration = NeedMath.ClampReserve(need.CurrentHydration + hydration, need.MaximumHydration)
        });
    }

    private static void ApplyToxicityExposure(Entity consumer, double toxicityExposure, EntityId sourceEntityId, ulong tick)
    {
        if (toxicityExposure <= 0)
        {
            return;
        }

        var exposure = new ContaminationExposure(toxicityExposure, 0, 0, sourceEntityId, null, tick);
        AddExposure(consumer, exposure);
    }

    internal static void AddExposure(Entity consumer, ContaminationExposure exposure)
    {
        var exposures = consumer.TryGetComponent<ContaminationExposureComponent>(out var component) && component is not null
            ? component.Exposures.ToList()
            : new List<ContaminationExposure>();

        exposures.Add(exposure);
        consumer.SetComponent(new ContaminationExposureComponent(exposures));
    }
}
