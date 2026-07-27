using Isekai.Engine.Core;
using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Resolves generic drinking intents from entity water sources and optional terrain water layers.
/// </summary>
public sealed class DrinkActionSystem : IWorldSystem
{
    private readonly TerrainWaterLayer? _terrainWaterLayer;

    /// <summary>
    /// Creates a drink action system without a terrain water layer.
    /// </summary>
    public DrinkActionSystem()
    {
    }

    /// <summary>
    /// Creates a drink action system with an optional terrain water layer.
    /// </summary>
    public DrinkActionSystem(TerrainWaterLayer? terrainWaterLayer)
    {
        _terrainWaterLayer = terrainWaterLayer;
    }

    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var intentEntity in context.World.EntitiesWith<DrinkFromEntityIntentComponent>())
        {
            var intent = intentEntity.GetComponent<DrinkFromEntityIntentComponent>().Intent;
            var result = ResolveEntityDrink(context, intent);

            intentEntity.SetComponent(new DrinkActionResultComponent(result));
            intentEntity.RemoveComponent<DrinkFromEntityIntentComponent>();
        }

        foreach (var intentEntity in context.World.EntitiesWith<DrinkFromTerrainIntentComponent>())
        {
            var intent = intentEntity.GetComponent<DrinkFromTerrainIntentComponent>().Intent;
            var result = ResolveTerrainDrink(context, intent);

            intentEntity.SetComponent(new DrinkActionResultComponent(result));
            intentEntity.RemoveComponent<DrinkFromTerrainIntentComponent>();
        }
    }

    private static DrinkActionResult ResolveEntityDrink(WorldSystemExecutionContext context, DrinkFromEntityIntent intent)
    {
        if (!NeedMath.IsPositiveFinite(intent.RequestedVolumeLiters))
        {
            return EmptyResult(intent.ConsumerEntityId, intent.SourceEntityId, null, intent.RequestedVolumeLiters, DrinkOutcome.InvalidQuantity);
        }

        if (!context.World.TryGetEntity(intent.ConsumerEntityId, out var consumer) || consumer is null)
        {
            return EmptyResult(intent.ConsumerEntityId, intent.SourceEntityId, null, intent.RequestedVolumeLiters, DrinkOutcome.MissingConsumer);
        }

        if (!context.World.TryGetEntity(intent.SourceEntityId, out var sourceEntity) || sourceEntity is null ||
            !sourceEntity.TryGetComponent<WaterSourceComponent>(out var source) ||
            source is null)
        {
            return EmptyResult(intent.ConsumerEntityId, intent.SourceEntityId, null, intent.RequestedVolumeLiters, DrinkOutcome.MissingResource);
        }

        if (!NeedsSpatial.IsWithinRange(consumer, sourceEntity, intent.MaximumDistanceMeters))
        {
            return EmptyResult(intent.ConsumerEntityId, intent.SourceEntityId, null, intent.RequestedVolumeLiters, DrinkOutcome.OutOfRange);
        }

        if (!consumer.HasComponent<HydrationNeedComponent>())
        {
            return EmptyResult(intent.ConsumerEntityId, intent.SourceEntityId, null, intent.RequestedVolumeLiters, DrinkOutcome.MissingHydrationNeed);
        }

        var available = NeedMath.ClampReserve(source.CurrentVolumeLiters, source.MaximumVolumeLiters);
        if (available <= 0)
        {
            return EmptyResult(intent.ConsumerEntityId, intent.SourceEntityId, null, intent.RequestedVolumeLiters, DrinkOutcome.ResourceEmpty);
        }

        var consumed = Math.Min(available, intent.RequestedVolumeLiters);
        sourceEntity.SetComponent(source with { CurrentVolumeLiters = Math.Max(0, available - consumed) });

        return ApplyDrinkResult(
            consumer,
            intent.ConsumerEntityId,
            intent.SourceEntityId,
            null,
            intent.RequestedVolumeLiters,
            consumed,
            source.Quality,
            context.Time.TickCount);
    }

    private DrinkActionResult ResolveTerrainDrink(WorldSystemExecutionContext context, DrinkFromTerrainIntent intent)
    {
        if (!NeedMath.IsPositiveFinite(intent.RequestedVolumeLiters))
        {
            return EmptyResult(intent.ConsumerEntityId, null, intent.SourceCoordinate, intent.RequestedVolumeLiters, DrinkOutcome.InvalidQuantity);
        }

        if (!context.World.TryGetEntity(intent.ConsumerEntityId, out var consumer) || consumer is null)
        {
            return EmptyResult(intent.ConsumerEntityId, null, intent.SourceCoordinate, intent.RequestedVolumeLiters, DrinkOutcome.MissingConsumer);
        }

        if (_terrainWaterLayer is null || !_terrainWaterLayer.TryGetWater(intent.SourceCoordinate, out var source))
        {
            return EmptyResult(intent.ConsumerEntityId, null, intent.SourceCoordinate, intent.RequestedVolumeLiters, DrinkOutcome.MissingResource);
        }

        if (!NeedsSpatial.IsWithinRange(consumer, intent.SourceCoordinate, intent.MaximumDistanceMeters))
        {
            return EmptyResult(intent.ConsumerEntityId, null, intent.SourceCoordinate, intent.RequestedVolumeLiters, DrinkOutcome.OutOfRange);
        }

        if (!consumer.HasComponent<HydrationNeedComponent>())
        {
            return EmptyResult(intent.ConsumerEntityId, null, intent.SourceCoordinate, intent.RequestedVolumeLiters, DrinkOutcome.MissingHydrationNeed);
        }

        if (source.AvailableVolumeLiters <= 0)
        {
            return EmptyResult(intent.ConsumerEntityId, null, intent.SourceCoordinate, intent.RequestedVolumeLiters, DrinkOutcome.ResourceEmpty);
        }

        var consumed = _terrainWaterLayer.ConsumeWater(intent.SourceCoordinate, intent.RequestedVolumeLiters);
        return ApplyDrinkResult(
            consumer,
            intent.ConsumerEntityId,
            null,
            intent.SourceCoordinate,
            intent.RequestedVolumeLiters,
            consumed,
            source.Quality,
            context.Time.TickCount);
    }

    private static DrinkActionResult ApplyDrinkResult(
        Entity consumer,
        EntityId consumerEntityId,
        EntityId? sourceEntityId,
        TerrainCellCoordinate? sourceCoordinate,
        double requestedVolumeLiters,
        double consumedVolumeLiters,
        WaterQuality quality,
        ulong tick)
    {
        var hydrationReceived = consumedVolumeLiters;
        var hydration = consumer.GetComponent<HydrationNeedComponent>();
        consumer.SetComponent(hydration with
        {
            CurrentHydration = NeedMath.ClampReserve(hydration.CurrentHydration + hydrationReceived, hydration.MaximumHydration)
        });

        var tolerance = consumer.TryGetComponent<WaterToleranceComponent>(out var component) && component is not null
            ? component
            : new WaterToleranceComponent(0, 0, 0);

        var bacterial = Math.Max(0, quality.BacterialContamination - tolerance.BacterialTolerance) * consumedVolumeLiters;
        var chemical = Math.Max(0, quality.ChemicalContamination - tolerance.ChemicalTolerance) * consumedVolumeLiters;
        var salinity = Math.Max(0, quality.Salinity - tolerance.SalinityTolerance) * consumedVolumeLiters;

        if (bacterial > 0 || chemical > 0 || salinity > 0)
        {
            ConsumeActionSystem.AddExposure(consumer, new ContaminationExposure(
                bacterial,
                chemical,
                salinity,
                sourceEntityId,
                sourceCoordinate,
                tick));
        }

        return new DrinkActionResult(
            consumerEntityId,
            sourceEntityId,
            sourceCoordinate,
            requestedVolumeLiters,
            consumedVolumeLiters,
            hydrationReceived,
            bacterial,
            chemical,
            salinity,
            consumedVolumeLiters < requestedVolumeLiters ? DrinkOutcome.PartialSuccess : DrinkOutcome.Success);
    }

    private static DrinkActionResult EmptyResult(
        EntityId consumerEntityId,
        EntityId? sourceEntityId,
        TerrainCellCoordinate? sourceCoordinate,
        double requestedVolumeLiters,
        DrinkOutcome outcome)
    {
        return new DrinkActionResult(consumerEntityId, sourceEntityId, sourceCoordinate, requestedVolumeLiters, 0, 0, 0, 0, 0, outcome);
    }
}
