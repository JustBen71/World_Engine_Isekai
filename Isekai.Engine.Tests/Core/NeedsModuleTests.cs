using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Core.System;
using Isekai.Engine.Data.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Exceptions;
using Isekai.Engine.Modules.Needs;
using Isekai.Engine.Modules.Terrain;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for needs, consumables, diet, water and terrain resource layers.
/// </summary>
public sealed class NeedsModuleTests
{
    [Fact]
    public void EnergyNeedSystem_DecreasesEnergyWithTime()
    {
        var world = CreateWorld();
        var entity = world.CreateEntity();
        entity.AddComponent(new EnergyNeedComponent(10, 20, 2));

        world.RegisterSystem(new EnergyNeedSystem());
        world.Tick(TimeSpan.FromSeconds(3));

        Assert.Equal(4, entity.GetComponent<EnergyNeedComponent>().CurrentEnergy);
    }

    [Fact]
    public void HydrationNeedSystem_DecreasesHydrationWithTime()
    {
        var world = CreateWorld();
        var entity = world.CreateEntity();
        entity.AddComponent(new HydrationNeedComponent(10, 20, 2));

        world.RegisterSystem(new HydrationNeedSystem());
        world.Tick(TimeSpan.FromSeconds(3));

        Assert.Equal(4, entity.GetComponent<HydrationNeedComponent>().CurrentHydration);
    }

    [Fact]
    public void PassiveNeeds_AreClampedAndIgnoreInvalidValues()
    {
        var world = CreateWorld();
        var energy = world.CreateEntity();
        var hydration = world.CreateEntity();
        var invalidMaximum = world.CreateEntity();
        var ignored = world.CreateEntity();

        energy.AddComponent(new EnergyNeedComponent(1, 10, 5));
        hydration.AddComponent(new HydrationNeedComponent(1, 10, 5));
        invalidMaximum.AddComponent(new EnergyNeedComponent(double.PositiveInfinity, -1, -5));

        world.RegisterSystem(new EnergyNeedSystem());
        world.RegisterSystem(new HydrationNeedSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.Equal(0, energy.GetComponent<EnergyNeedComponent>().CurrentEnergy);
        Assert.Equal(0, hydration.GetComponent<HydrationNeedComponent>().CurrentHydration);
        Assert.Equal(0, invalidMaximum.GetComponent<EnergyNeedComponent>().CurrentEnergy);
        Assert.Empty(ignored.Components);
    }

    [Fact]
    public void PassiveNeeds_HandleZeroConsumptionNegativeDeltaAndDeterminism()
    {
        var first = CreateWorld();
        var second = CreateWorld();
        var firstEntity = first.CreateEntity();
        var secondEntity = second.CreateEntity();
        firstEntity.AddComponent(new EnergyNeedComponent(10, 10, 0));
        secondEntity.AddComponent(new EnergyNeedComponent(10, 10, 0));

        var system = new EnergyNeedSystem();
        system.Execute(new WorldSystemExecutionContext(first, TimeSpan.FromSeconds(-10), first.Time, first.EventBus));
        first.RegisterSystem(new EnergyNeedSystem());
        second.RegisterSystem(new EnergyNeedSystem());

        for (var i = 0; i < 5; i++)
        {
            first.Tick(TimeSpan.FromSeconds(1));
            second.Tick(TimeSpan.FromSeconds(1));
        }

        Assert.Equal(10, firstEntity.GetComponent<EnergyNeedComponent>().CurrentEnergy);
        Assert.Equal(firstEntity.GetComponent<EnergyNeedComponent>(), secondEntity.GetComponent<EnergyNeedComponent>());
    }

    [Fact]
    public void ConsumableDefinitions_LoadAndValidateJson()
    {
        var registry = LoadNeedsDefinitions(ValidConsumableJson("consumable.berries", "fruit", energy: 12));
        var definition = registry.Get<ConsumableDefinition>(DefinitionId.From("consumable.berries"));

        Assert.True(registry.IsFrozen);
        Assert.Equal(12, definition.EnergyPerUnit);
        Assert.Contains("fruit", definition.Tags);
    }

    [Fact]
    public void ConsumableDefinitions_RejectInvalidData()
    {
        Assert.Throws<InvalidDefinitionDataException>(() => LoadNeedsDefinitions(ValidConsumableJson("consumable.bad", "fruit", digestibility: 1.5)));
        Assert.Throws<InvalidDefinitionDataException>(() => LoadNeedsDefinitions(ValidConsumableJson("consumable.bad", "fruit", energy: -1)));
        Assert.Throws<InvalidDefinitionDataException>(() => LoadNeedsDefinitions(ValidConsumableJson("consumable.bad", "fruit", energy: double.PositiveInfinity)));
        AssertInvalidConsumable(CreateConsumable("consumable.nan", "fruit", energy: double.NaN));
        Assert.Throws<InvalidDefinitionDataException>(() => LoadNeedsDefinitions("""
            {
              "type": "needs.consumable",
              "id": "consumable.duplicate",
              "displayName": "Duplicate",
              "tags": [ "fruit", "fruit" ],
              "energyPerUnit": 1,
              "hydrationPerUnit": 1,
              "toxicityPerUnit": 0,
              "defaultDigestibility": 1
            }
            """));
    }

    [Fact]
    public void ConsumableReferenceUnknown_IsRejectedByRegistryValidation()
    {
        var registry = new DefinitionRegistry();
        registry.Register(new ResourceReferencingDefinition(
            DefinitionId.From("owner"),
            DefinitionReference<ConsumableDefinition>.From("consumable.missing")));

        Assert.Throws<DefinitionValidationException>(() => registry.EnsureReferencesValid());
    }

    [Fact]
    public void FrozenRegistry_RejectsAdditionalConsumableDefinitions()
    {
        var registry = LoadNeedsDefinitions(ValidConsumableJson("consumable.berries", "fruit"));

        Assert.Throws<DefinitionRegistryFrozenException>(() => registry.Register(CreateConsumable("consumable.other", "fruit")));
    }

    [Fact]
    public void ConsumeActionSystem_ConsumesFoodAndUpdatesNeeds()
    {
        var world = CreateWorld();
        var consumer = CreateConsumer(world, energy: 0, hydration: 0, fruitDigestibility: 1);
        var resource = CreateResource(world, "consumable.berries", quantity: 10);

        consumer.AddComponent(new ConsumeIntentComponent(new ConsumeIntent(consumer.Id, resource.Id, 2)));
        world.RegisterSystem(new ConsumeActionSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var result = consumer.GetComponent<ConsumeActionResultComponent>().Result;
        Assert.Equal(ConsumeOutcome.Success, result.Outcome);
        Assert.Equal(2, result.ConsumedQuantity);
        Assert.Equal(24, consumer.GetComponent<EnergyNeedComponent>().CurrentEnergy);
        Assert.Equal(8, consumer.GetComponent<HydrationNeedComponent>().CurrentHydration);
        Assert.Equal(8, resource.GetComponent<ConsumableResourceComponent>().CurrentQuantity);
    }

    [Fact]
    public void ConsumeActionSystem_PartialConsumptionClampsNeedsAndStock()
    {
        var world = CreateWorld();
        var consumer = CreateConsumer(world, energy: 95, hydration: 98, fruitDigestibility: 1);
        var resource = CreateResource(world, "consumable.berries", quantity: 1);

        consumer.AddComponent(new ConsumeIntentComponent(new ConsumeIntent(consumer.Id, resource.Id, 5)));
        world.RegisterSystem(new ConsumeActionSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.Equal(ConsumeOutcome.PartialSuccess, consumer.GetComponent<ConsumeActionResultComponent>().Result.Outcome);
        Assert.Equal(0, resource.GetComponent<ConsumableResourceComponent>().CurrentQuantity);
        Assert.Equal(100, consumer.GetComponent<EnergyNeedComponent>().CurrentEnergy);
        Assert.Equal(100, consumer.GetComponent<HydrationNeedComponent>().CurrentHydration);
    }

    [Fact]
    public void ConsumeActionSystem_RefusesIncompatiblePartialAndInvalidResources()
    {
        var world = CreateWorld();
        var consumer = CreateConsumer(world, energy: 0, hydration: 0, fruitDigestibility: 0.5, meatDigestibility: 0);
        var fruit = CreateResource(world, "consumable.berries", quantity: 10);
        var meat = CreateResource(world, "consumable.meat", quantity: 10);

        consumer.AddComponent(new ConsumeIntentComponent(new ConsumeIntent(consumer.Id, fruit.Id, 2)));
        world.RegisterSystem(new ConsumeActionSystem());
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(12, consumer.GetComponent<EnergyNeedComponent>().CurrentEnergy);

        consumer.AddComponent(new ConsumeIntentComponent(new ConsumeIntent(consumer.Id, meat.Id, 2)));
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(ConsumeOutcome.NotDigestible, consumer.GetComponent<ConsumeActionResultComponent>().Result.Outcome);

        consumer.AddComponent(new ConsumeIntentComponent(new ConsumeIntent(consumer.Id, fruit.Id, 0)));
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(ConsumeOutcome.InvalidQuantity, consumer.GetComponent<ConsumeActionResultComponent>().Result.Outcome);
    }

    [Fact]
    public void ConsumeActionSystem_ReportsMissingAndEmptyCasesAndCanRemoveExhaustedResource()
    {
        var world = CreateWorld();
        var consumer = CreateConsumer(world, energy: 0, hydration: 0, fruitDigestibility: 1);
        var empty = CreateResource(world, "consumable.berries", quantity: 0);
        var removable = CreateResource(world, "consumable.berries", quantity: 1, removeWhenEmpty: true);
        var intentCarrier = world.CreateEntity();

        consumer.AddComponent(new ConsumeIntentComponent(new ConsumeIntent(consumer.Id, empty.Id, 1)));
        world.RegisterSystem(new ConsumeActionSystem());
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(ConsumeOutcome.ResourceEmpty, consumer.GetComponent<ConsumeActionResultComponent>().Result.Outcome);

        intentCarrier.AddComponent(new ConsumeIntentComponent(new ConsumeIntent(EntityId.New(), empty.Id, 1)));
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(ConsumeOutcome.MissingConsumer, intentCarrier.GetComponent<ConsumeActionResultComponent>().Result.Outcome);

        intentCarrier.AddComponent(new ConsumeIntentComponent(new ConsumeIntent(consumer.Id, EntityId.New(), 1)));
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(ConsumeOutcome.MissingResource, intentCarrier.GetComponent<ConsumeActionResultComponent>().Result.Outcome);

        consumer.AddComponent(new ConsumeIntentComponent(new ConsumeIntent(consumer.Id, removable.Id, 1)));
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.False(world.ContainsEntity(removable.Id));
    }

    [Fact]
    public void ConsumableResource_DoesNotRequireBodyAndCanRegenerate()
    {
        var world = CreateWorld();
        var resource = CreateResource(world, "consumable.berries", quantity: 1, maximum: 5, regeneration: 2);

        world.RegisterSystem(new ConsumableRegenerationSystem());
        world.Tick(TimeSpan.FromSeconds(3));

        Assert.Equal(5, resource.GetComponent<ConsumableResourceComponent>().CurrentQuantity);
    }

    [Fact]
    public void DrinkActionSystem_CleanAndContaminatedWaterUpdateHydrationAndExposure()
    {
        var world = CreateWorld();
        var consumer = CreateConsumer(world, energy: 0, hydration: 0, fruitDigestibility: 1);
        consumer.AddComponent(new WaterToleranceComponent(0.2, 0.2, 0.2));
        var clean = CreateWater(world, 10, WaterQuality.Clean);
        var dirty = CreateWater(world, 10, new WaterQuality(0.6, 0.1, 0.8));

        consumer.AddComponent(new DrinkFromEntityIntentComponent(new DrinkFromEntityIntent(consumer.Id, clean.Id, 2)));
        world.RegisterSystem(new DrinkActionSystem());
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(2, consumer.GetComponent<HydrationNeedComponent>().CurrentHydration);
        Assert.False(consumer.HasComponent<ContaminationExposureComponent>());

        consumer.AddComponent(new DrinkFromEntityIntentComponent(new DrinkFromEntityIntent(consumer.Id, dirty.Id, 2)));
        world.Tick(TimeSpan.FromSeconds(1));

        var result = consumer.GetComponent<DrinkActionResultComponent>().Result;
        Assert.Equal(DrinkOutcome.Success, result.Outcome);
        Assert.Equal(0.8, result.BacterialExposure, 6);
        Assert.Equal(1.2, result.SalinityExposure, 6);
        Assert.Single(consumer.GetComponent<ContaminationExposureComponent>().Exposures);
    }

    [Fact]
    public void DrinkActionSystem_HandlesTolerancePartialInvalidMissingAndDeterminism()
    {
        var first = CreateWorld();
        var second = CreateWorld();
        var firstConsumer = CreateConsumer(first, 0, 98, fruitDigestibility: 1);
        var secondConsumer = CreateConsumer(second, 0, 98, fruitDigestibility: 1);
        firstConsumer.AddComponent(new WaterToleranceComponent(1, 1, 1));
        secondConsumer.AddComponent(new WaterToleranceComponent(1, 1, 1));
        var firstSource = CreateWater(first, 1, new WaterQuality(0.5, 0.5, 0.5));
        var secondSource = CreateWater(second, 1, new WaterQuality(0.5, 0.5, 0.5));

        firstConsumer.AddComponent(new DrinkFromEntityIntentComponent(new DrinkFromEntityIntent(firstConsumer.Id, firstSource.Id, 5)));
        secondConsumer.AddComponent(new DrinkFromEntityIntentComponent(new DrinkFromEntityIntent(secondConsumer.Id, secondSource.Id, 5)));
        first.RegisterSystem(new DrinkActionSystem());
        second.RegisterSystem(new DrinkActionSystem());
        first.Tick(TimeSpan.FromSeconds(1));
        second.Tick(TimeSpan.FromSeconds(1));

        Assert.Equal(DrinkOutcome.PartialSuccess, firstConsumer.GetComponent<DrinkActionResultComponent>().Result.Outcome);
        Assert.Equal(99, firstConsumer.GetComponent<HydrationNeedComponent>().CurrentHydration);
        Assert.Equal(0, firstSource.GetComponent<WaterSourceComponent>().CurrentVolumeLiters);
        Assert.Equal(firstConsumer.GetComponent<HydrationNeedComponent>(), secondConsumer.GetComponent<HydrationNeedComponent>());
        Assert.False(firstConsumer.HasComponent<ContaminationExposureComponent>());

        firstConsumer.AddComponent(new DrinkFromEntityIntentComponent(new DrinkFromEntityIntent(firstConsumer.Id, firstSource.Id, -1)));
        first.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(DrinkOutcome.InvalidQuantity, firstConsumer.GetComponent<DrinkActionResultComponent>().Result.Outcome);

        firstConsumer.AddComponent(new DrinkFromEntityIntentComponent(new DrinkFromEntityIntent(EntityId.New(), firstSource.Id, 1)));
        first.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(DrinkOutcome.MissingConsumer, firstConsumer.GetComponent<DrinkActionResultComponent>().Result.Outcome);

        firstConsumer.AddComponent(new DrinkFromEntityIntentComponent(new DrinkFromEntityIntent(firstConsumer.Id, EntityId.New(), 1)));
        first.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(DrinkOutcome.MissingResource, firstConsumer.GetComponent<DrinkActionResultComponent>().Result.Outcome);
    }

    [Fact]
    public void WaterSourceRefillSystem_ClampsRefill()
    {
        var world = CreateWorld();
        var source = CreateWater(world, 1, WaterQuality.Clean, maximum: 5, refill: 10);

        world.RegisterSystem(new WaterSourceRefillSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.Equal(5, source.GetComponent<WaterSourceComponent>().CurrentVolumeLiters);
    }

    [Fact]
    public void WaterQuality_RejectsInvalidValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WaterQuality(-0.1, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WaterQuality(double.NaN, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WaterQuality(0, double.PositiveInfinity, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WaterQuality(0, 0, 1.1));
    }

    [Fact]
    public void TerrainBiomassLayer_StoresConsumesAndClampsBiomass()
    {
        var layer = new TerrainBiomassLayer(DefinitionReference<ConsumableDefinition>.From("consumable.grass"));
        var first = new TerrainCellCoordinate(1, 1);
        var second = new TerrainCellCoordinate(2, 2);

        layer.SetBiomass(first, 5);
        layer.SetBiomass(second, 10);

        Assert.Equal(3, layer.ConsumeBiomass(first, 3));
        Assert.Equal(2, layer.GetBiomass(first));
        Assert.Equal(2, layer.ConsumeBiomass(first, 10));
        Assert.Equal(0, layer.GetBiomass(first));
        Assert.Equal(10, layer.GetBiomass(second));
        Assert.Equal(0, layer.GetBiomass(new TerrainCellCoordinate(100, 100)));
        Assert.Throws<ArgumentOutOfRangeException>(() => layer.SetBiomass(first, double.NaN));
    }

    [Fact]
    public void TerrainWaterLayer_StoresConsumesAndSeparatesCells()
    {
        var layer = new TerrainWaterLayer();
        var clean = new TerrainCellCoordinate(0, 0);
        var salty = new TerrainCellCoordinate(1, 0);

        layer.SetWater(clean, 5, WaterQuality.Clean);
        layer.SetWater(salty, 10, new WaterQuality(0, 0, 1));

        Assert.Equal(3, layer.ConsumeWater(clean, 3));
        Assert.True(layer.TryGetWater(clean, out var cleanResource));
        Assert.True(layer.TryGetWater(salty, out var saltyResource));
        Assert.Equal(2, cleanResource.AvailableVolumeLiters);
        Assert.Equal(10, saltyResource.AvailableVolumeLiters);
        Assert.Equal(1, saltyResource.Quality.Salinity);
        Assert.Equal(0, layer.ConsumeWater(new TerrainCellCoordinate(-1, -1), 1));
    }

    [Fact]
    public void DrinkActionSystem_CanDrinkFromTerrainWaterLayerUsingPositionComponent()
    {
        var layer = new TerrainWaterLayer();
        var coordinate = new TerrainCellCoordinate(1, 1);
        layer.SetWater(coordinate, 5, new WaterQuality(0.5, 0, 0));
        var world = CreateWorld();
        var consumer = CreateConsumer(world, 0, 0, fruitDigestibility: 1);
        consumer.AddComponent(new PositionComponent(new WorldPosition(1, 1, 0)));
        consumer.AddComponent(new WaterToleranceComponent(0, 0, 0));

        consumer.AddComponent(new DrinkFromTerrainIntentComponent(new DrinkFromTerrainIntent(consumer.Id, coordinate, 2)));
        world.RegisterSystem(new DrinkActionSystem(layer));
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.Equal(2, consumer.GetComponent<HydrationNeedComponent>().CurrentHydration);
        Assert.Equal(3, layer.TryGetWater(coordinate, out var water) ? water.AvailableVolumeLiters : -1);
        Assert.True(consumer.GetComponent<ContaminationExposureComponent>().Exposures.Single().BacterialExposure > 0);
    }

    [Fact]
    public void NeedsModule_RegistersDefinitionsAndSystems()
    {
        var module = new NeedsModule();
        var typeRegistry = new DefinitionTypeRegistry();
        var world = CreateWorld();

        module.RegisterDefinitionTypes(typeRegistry);
        module.RegisterSystems(world);

        Assert.Contains("needs.consumable", typeRegistry.TypeNames);
        Assert.Contains(world.Systems, system => system is EnergyNeedSystem);
        Assert.Contains(world.Systems, system => system is DrinkActionSystem);
    }

    private static WorldState CreateWorld()
    {
        return new WorldState(
            new SimulationTime(),
            new EventBus(),
            CreateRegistry(),
            NoOpTraceLogger.Instance);
    }

    private static DefinitionRegistry CreateRegistry()
    {
        var registry = new DefinitionRegistry();
        registry.Register(CreateConsumable("consumable.berries", "fruit", energy: 12, hydration: 4));
        registry.Register(CreateConsumable("consumable.grass", "grass", energy: 4, hydration: 1));
        registry.Register(CreateConsumable("consumable.meat", "meat", energy: 20, hydration: 1));
        registry.Freeze();
        return registry;
    }

    private static ConsumableDefinition CreateConsumable(
        string id,
        string tag,
        double energy = 1,
        double hydration = 1,
        double toxicity = 0,
        double digestibility = 1)
    {
        return new ConsumableDefinition(
            DefinitionId.From(id),
            id,
            new[] { tag },
            energy,
            hydration,
            toxicity,
            digestibility);
    }

    private static Entity CreateConsumer(
        WorldState world,
        double energy,
        double hydration,
        double fruitDigestibility,
        double meatDigestibility = 1)
    {
        var entity = world.CreateEntity();
        entity.AddComponent(new EnergyNeedComponent(energy, 100, 0));
        entity.AddComponent(new HydrationNeedComponent(hydration, 100, 0));
        entity.AddComponent(new DietComponent(new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["fruit"] = fruitDigestibility,
            ["grass"] = fruitDigestibility,
            ["meat"] = meatDigestibility
        }));
        return entity;
    }

    private static Entity CreateResource(
        WorldState world,
        string definitionId,
        double quantity,
        double maximum = 10,
        double regeneration = 0,
        bool removeWhenEmpty = false)
    {
        var entity = world.CreateEntity();
        entity.AddComponent(new ConsumableResourceComponent(
            DefinitionReference<ConsumableDefinition>.From(definitionId),
            quantity,
            maximum,
            regeneration,
            removeWhenEmpty));
        return entity;
    }

    private static Entity CreateWater(
        WorldState world,
        double volume,
        WaterQuality quality,
        double maximum = 10,
        double refill = 0)
    {
        var entity = world.CreateEntity();
        entity.AddComponent(new WaterSourceComponent(volume, maximum, quality, refill));
        return entity;
    }

    private static DefinitionRegistry LoadNeedsDefinitions(string json)
    {
        var typeRegistry = new DefinitionTypeRegistry(NoOpTraceLogger.Instance);
        typeRegistry.RegisterNeedsDefinitions();
        var pipeline = new DefinitionLoadPipeline(
            new JsonDefinitionLoader(typeRegistry, NoOpTraceLogger.Instance),
            new[] { new ConsumableDefinitionValidator() },
            NoOpTraceLogger.Instance);
        return pipeline.LoadJson(json);
    }

    private static void AssertInvalidConsumable(ConsumableDefinition definition)
    {
        var registry = new DefinitionRegistry();
        registry.Register(definition);

        Assert.Throws<InvalidDefinitionDataException>(() => new ConsumableDefinitionValidator().Validate(registry));
    }

    private static string ValidConsumableJson(
        string id,
        string tag,
        double energy = 1,
        double hydration = 1,
        double toxicity = 0,
        double digestibility = 1)
    {
        return $$"""
            {
              "type": "needs.consumable",
              "id": "{{id}}",
              "displayName": "{{id}}",
              "tags": [ "{{tag}}" ],
              "energyPerUnit": {{FormatDouble(energy)}},
              "hydrationPerUnit": {{FormatDouble(hydration)}},
              "toxicityPerUnit": {{FormatDouble(toxicity)}},
              "defaultDigestibility": {{FormatDouble(digestibility)}}
            }
            """;
    }

    private static string FormatDouble(double value)
    {
        if (double.IsNaN(value))
        {
            return "NaN";
        }

        if (double.IsPositiveInfinity(value))
        {
            return "1e999";
        }

        return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private sealed record ResourceReferencingDefinition(
        DefinitionId Id,
        DefinitionReference<ConsumableDefinition> Consumable) : IDefinition;
}
