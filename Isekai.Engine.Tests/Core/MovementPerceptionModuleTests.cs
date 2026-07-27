using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Modules.Movement;
using Isekai.Engine.Modules.Needs;
using Isekai.Engine.Modules.Perception;
using Isekai.Engine.Modules.Temperature;
using Isekai.Engine.Modules.Terrain;
using Isekai.Engine.Modules.Vitals;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for movement, perception and their integration loop.
/// </summary>
public sealed class MovementPerceptionModuleTests
{
    [Fact]
    public void MovementResolutionSystem_MovesOnFlatTerrainAndUpdatesPosition()
    {
        var world = CreateWorld(out var terrain);
        var entity = CreateMover(world, new WorldPosition(1, 1, 0));

        entity.AddComponent(new MoveIntentComponent(new MoveIntent(entity.Id, new WorldVector(1, 0, 0), 2, MovementMode.Walk)));
        world.RegisterSystem(new MovementResolutionSystem(terrain));
        world.Tick(TimeSpan.FromSeconds(2));

        var result = entity.GetComponent<MovementResultComponent>().Result;
        Assert.Equal(MovementOutcome.Success, result.Outcome);
        Assert.Equal(3, entity.GetComponent<PositionComponent>().Position.X, 6);
        Assert.Equal(2, result.ActualDistanceMeters, 6);
    }

    [Fact]
    public void MovementResolutionSystem_LimitsDistanceBySpeedAndDelta()
    {
        var world = CreateWorld(out var terrain);
        var entity = CreateMover(world, new WorldPosition(1, 1, 0), speed: 1);

        entity.AddComponent(new MoveIntentComponent(new MoveIntent(entity.Id, new WorldVector(10, 0, 0), 10, MovementMode.Walk)));
        world.RegisterSystem(new MovementResolutionSystem(terrain));
        world.Tick(TimeSpan.FromSeconds(2));

        var result = entity.GetComponent<MovementResultComponent>().Result;
        Assert.Equal(MovementOutcome.PartialSuccess, result.Outcome);
        Assert.Equal(2, result.ActualDistanceMeters, 6);
        Assert.Equal(3, entity.GetComponent<PositionComponent>().Position.X, 6);
    }

    [Fact]
    public void MovementResolutionSystem_RejectsInvalidIntentValues()
    {
        var world = CreateWorld(out var terrain);
        var entity = CreateMover(world, new WorldPosition(1, 1, 0));

        entity.AddComponent(new MoveIntentComponent(new MoveIntent(entity.Id, new WorldVector(0, 0, 0), 1, MovementMode.Walk)));
        world.RegisterSystem(new MovementResolutionSystem(terrain));
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(MovementOutcome.InvalidIntent, entity.GetComponent<MovementResultComponent>().Result.Outcome);

        entity.AddComponent(new MoveIntentComponent(new MoveIntent(entity.Id, new WorldVector(1, 0, 0), -1, MovementMode.Walk)));
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(MovementOutcome.InvalidIntent, entity.GetComponent<MovementResultComponent>().Result.Outcome);

        entity.AddComponent(new MoveIntentComponent(new MoveIntent(entity.Id, new WorldVector(double.NaN, 0, 0), 1, MovementMode.Walk)));
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(MovementOutcome.InvalidIntent, entity.GetComponent<MovementResultComponent>().Result.Outcome);

        entity.AddComponent(new MoveIntentComponent(new MoveIntent(entity.Id, new WorldVector(1, 0, 0), 1, (MovementMode)999)));
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(MovementOutcome.InvalidIntent, entity.GetComponent<MovementResultComponent>().Result.Outcome);
    }

    [Fact]
    public void MovementResolutionSystem_ReportsMissingCases()
    {
        var world = CreateWorld(out var terrain);
        var carrier = world.CreateEntity();
        carrier.AddComponent(new MoveIntentComponent(new MoveIntent(EntityId.New(), new WorldVector(1, 0, 0), 1, MovementMode.Walk)));

        world.RegisterSystem(new MovementResolutionSystem(terrain));
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(MovementOutcome.MissingEntity, carrier.GetComponent<MovementResultComponent>().Result.Outcome);

        var noPosition = world.CreateEntity();
        noPosition.AddComponent(new MovementCapabilityComponent(1, 1, 1, 1));
        noPosition.AddComponent(new MoveIntentComponent(new MoveIntent(noPosition.Id, new WorldVector(1, 0, 0), 1, MovementMode.Walk)));
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(MovementOutcome.MissingPosition, noPosition.GetComponent<MovementResultComponent>().Result.Outcome);

        var noCapability = world.CreateEntity();
        noCapability.AddComponent(new PositionComponent(new WorldPosition(1, 1, 0)));
        noCapability.AddComponent(new MoveIntentComponent(new MoveIntent(noCapability.Id, new WorldVector(1, 0, 0), 1, MovementMode.Walk)));
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(MovementOutcome.MissingCapability, noCapability.GetComponent<MovementResultComponent>().Result.Outcome);
    }

    [Fact]
    public void MovementResolutionSystem_HandlesBoundsSlopeTerrainObstacleAndMobility()
    {
        var world = CreateWorld(out var terrain);
        terrain.Grid.SetElevation(new TerrainCellCoordinate(2, 1), 5);
        var steep = CreateMover(world, new WorldPosition(1, 1, 0), maxSlope: 0.1);
        steep.AddComponent(new MoveIntentComponent(new MoveIntent(steep.Id, new WorldVector(1, 0, 0), 1, MovementMode.Walk)));
        world.RegisterSystem(new MovementResolutionSystem(terrain));
        world.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(MovementOutcome.SlopeTooSteep, steep.GetComponent<MovementResultComponent>().Result.Outcome);

        var obstacleWorld = CreateWorld(out var obstacleTerrain);
        var obstacle = CreateMover(obstacleWorld, new WorldPosition(1, 2, 0));
        obstacle.AddComponent(new MoveIntentComponent(new MoveIntent(obstacle.Id, new WorldVector(1, 0, 0), 1, MovementMode.Walk)));
        obstacleWorld.RegisterSystem(new MovementResolutionSystem(
            obstacleTerrain,
            traversalProvider: new TerrainTraversalProvider(obstacleTerrain, new[] { new TerrainCellCoordinate(2, 2) })));
        obstacleWorld.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(MovementOutcome.ImpassableTerrain, obstacle.GetComponent<MovementResultComponent>().Result.Outcome);

        var mobilityWorld = CreateWorld(out var mobilityTerrain);
        var noMobility = CreateMover(mobilityWorld, new WorldPosition(1, 3, 0));
        noMobility.AddComponent(new MobilityModifierComponent(0));
        noMobility.AddComponent(new MoveIntentComponent(new MoveIntent(noMobility.Id, new WorldVector(1, 0, 0), 1, MovementMode.Walk)));
        mobilityWorld.RegisterSystem(new MovementResolutionSystem(mobilityTerrain));
        mobilityWorld.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(MovementOutcome.NoMobility, noMobility.GetComponent<MovementResultComponent>().Result.Outcome);

        var boundsWorld = CreateWorld(out var boundsTerrain);
        var outOfBounds = CreateMover(boundsWorld, new WorldPosition(9, 9, 0));
        outOfBounds.AddComponent(new MoveIntentComponent(new MoveIntent(outOfBounds.Id, new WorldVector(1, 0, 0), 5, MovementMode.Walk)));
        boundsWorld.RegisterSystem(new MovementResolutionSystem(boundsTerrain));
        boundsWorld.Tick(TimeSpan.FromSeconds(1));
        Assert.Equal(MovementOutcome.OutOfBounds, outOfBounds.GetComponent<MovementResultComponent>().Result.Outcome);
    }

    [Fact]
    public void MovementResolutionSystem_AppliesSoilSlowdownCostsAndInsufficientEnergy()
    {
        var world = CreateWorld(out var terrain);
        terrain.Grid.SetSoil(new TerrainCellCoordinate(2, 1), DefinitionId.From("soil.mud"));
        var entity = CreateMover(world, new WorldPosition(1, 1, 0), speed: 10, energy: 1, hydration: 10, energyCost: 1);

        entity.AddComponent(new MoveIntentComponent(new MoveIntent(entity.Id, new WorldVector(1, 0, 0), 5, MovementMode.Walk)));
        world.RegisterSystem(new MovementResolutionSystem(terrain));
        world.Tick(TimeSpan.FromSeconds(1));

        var result = entity.GetComponent<MovementResultComponent>().Result;
        Assert.Equal(MovementOutcome.PartialSuccess, result.Outcome);
        Assert.True(result.ActualDistanceMeters < 5);
        Assert.Equal(0, entity.GetComponent<EnergyNeedComponent>().CurrentEnergy);
        Assert.True(entity.GetComponent<HydrationNeedComponent>().CurrentHydration < 10);
    }

    [Fact]
    public void MovementResolutionSystem_IsDeterministic()
    {
        var first = CreateWorld(out var firstTerrain);
        var second = CreateWorld(out var secondTerrain);
        var firstEntity = CreateMover(first, new WorldPosition(1, 1, 0));
        var secondEntity = CreateMover(second, new WorldPosition(1, 1, 0));

        firstEntity.AddComponent(new MoveIntentComponent(new MoveIntent(firstEntity.Id, new WorldVector(1, 1, 0), 2, MovementMode.Run)));
        secondEntity.AddComponent(new MoveIntentComponent(new MoveIntent(secondEntity.Id, new WorldVector(1, 1, 0), 2, MovementMode.Run)));
        first.RegisterSystem(new MovementResolutionSystem(firstTerrain));
        second.RegisterSystem(new MovementResolutionSystem(secondTerrain));
        first.Tick(TimeSpan.FromSeconds(1));
        second.Tick(TimeSpan.FromSeconds(1));

        Assert.Equal(firstEntity.GetComponent<PositionComponent>(), secondEntity.GetComponent<PositionComponent>());
        Assert.Equal(firstEntity.GetComponent<MovementResultComponent>().Result.ActualDistanceMeters,
            secondEntity.GetComponent<MovementResultComponent>().Result.ActualDistanceMeters);
    }

    [Fact]
    public void AgentObservationBuilder_PerceivesEntitiesInRangeDeterministically()
    {
        var world = CreateWorld(out _);
        var observer = CreateObserver(world, new WorldPosition(1, 1, 0), range: 10, limit: 10);
        var food = CreatePerceivedFood(world, new WorldPosition(4, 1, 0), "resource", "food");
        var water = CreatePerceivedWater(world, new WorldPosition(2, 1, 0), "resource", "water");
        var animal = CreatePerceived(world, new WorldPosition(3, 1, 0), "living");
        CreatePerceived(world, new WorldPosition(20, 1, 0), "far");
        var noPosition = world.CreateEntity();
        noPosition.AddComponent(new PerceptionSignatureComponent(new[] { "ignored" }));
        var noSignature = world.CreateEntity();
        noSignature.AddComponent(new PositionComponent(new WorldPosition(2, 2, 0)));

        var observation = new AgentObservationBuilder().BuildObservation(world, observer.Id);

        Assert.DoesNotContain(observation.PerceivedEntities, item => item.EntityId == observer.Id);
        Assert.Contains(observation.PerceivedEntities, item => item.EntityId == food.Id && item.AvailableFoodQuantity == 10);
        Assert.Contains(observation.PerceivedEntities, item => item.EntityId == water.Id && item.AvailableWaterLiters == 10);
        Assert.Contains(observation.PerceivedEntities, item => item.EntityId == animal.Id);
        Assert.Equal(water.Id, observation.PerceivedEntities[0].EntityId);
        Assert.Equal(1, observation.PerceivedEntities[0].DistanceMeters, 6);
        Assert.Equal(new WorldVector(1, 0, 0), observation.PerceivedEntities[0].Direction);
    }

    [Fact]
    public void AgentObservationBuilder_LimitsResultsAndNormalizesInternalState()
    {
        var world = CreateWorld(out _);
        var observer = CreateObserver(world, new WorldPosition(1, 1, 0), range: 10, limit: 1);
        observer.AddComponent(new EnergyNeedComponent(50, 100, 0));
        observer.AddComponent(new HydrationNeedComponent(25, 100, 0));
        observer.AddComponent(new MobilityModifierComponent(0.5));
        observer.AddComponent(new ThermalComfortComponent(0.75, 0, 0));
        observer.AddComponent(new VitalStateComponent(true));
        CreatePerceived(world, new WorldPosition(2, 1, 0), "a");
        CreatePerceived(world, new WorldPosition(3, 1, 0), "b");

        var observation = new AgentObservationBuilder().BuildObservation(world, observer.Id);

        Assert.Single(observation.PerceivedEntities);
        Assert.Equal(0.5, observation.InternalState.EnergyNormalized);
        Assert.Equal(0.25, observation.InternalState.HydrationNormalized);
        Assert.Equal(0.5, observation.InternalState.MobilityNormalized);
        Assert.Equal(0.75, observation.InternalState.ThermalComfortNormalized);
        Assert.True(observation.InternalState.IsAlive);
    }

    [Fact]
    public void AgentObservationBuilder_PerceivesTerrainBiomassAndWater()
    {
        var world = CreateWorld(out var terrain);
        var observer = CreateObserver(world, new WorldPosition(1, 1, 0), range: 5, limit: 10);
        var biomass = new TerrainBiomassLayer(DefinitionReference<ConsumableDefinition>.From("consumable.grass"));
        var water = new TerrainWaterLayer();
        biomass.SetBiomass(new TerrainCellCoordinate(2, 1), 4);
        water.SetWater(new TerrainCellCoordinate(1, 2), 8, WaterQuality.Clean);

        var provider = new NeedsTerrainResourcePerceptionProvider(terrain.Grid, biomass, water);
        var observation = new AgentObservationBuilder(terrainResourceProvider: provider).BuildObservation(world, observer.Id);

        Assert.Contains(observation.PerceivedTerrain, item => item.Tags.Contains("biomass") && item.Quantity == 4);
        Assert.Contains(observation.PerceivedTerrain, item => item.Tags.Contains("water") && item.Quantity == 8);
    }

    [Fact]
    public void AgentObservationBuilder_ReturnsNoPerceptionsWhenNoneAvailableAndHandlesDifferentRanges()
    {
        var nearWorld = CreateWorld(out _);
        var nearObserver = CreateObserver(nearWorld, new WorldPosition(1, 1, 0), range: 2, limit: 10);
        CreatePerceived(nearWorld, new WorldPosition(5, 1, 0), "resource");

        var builder = new AgentObservationBuilder();
        Assert.Empty(builder.BuildObservation(nearWorld, nearObserver.Id).PerceivedEntities);

        var farWorld = CreateWorld(out _);
        var farObserver = CreateObserver(farWorld, new WorldPosition(1, 1, 0), range: 10, limit: 10);
        var target = CreatePerceived(farWorld, new WorldPosition(5, 1, 0), "resource");
        Assert.Contains(builder.BuildObservation(farWorld, farObserver.Id).PerceivedEntities, item => item.EntityId == target.Id);
    }

    [Fact]
    public void MovementAndPerceptionLoop_MovesCloserToWaterAndCostsNeeds()
    {
        var world = CreateWorld(out var terrain);
        var agent = CreateObserver(world, new WorldPosition(1, 1, 0), range: 10, limit: 10);
        agent.AddComponent(new MovementCapabilityComponent(2, 1, 1, 1));
        agent.AddComponent(new EnergyNeedComponent(100, 100, 0));
        agent.AddComponent(new HydrationNeedComponent(100, 100, 0));
        var water = CreatePerceivedWater(world, new WorldPosition(5, 1, 0), "water");
        var builder = new AgentObservationBuilder();
        var before = builder.BuildObservation(world, agent.Id).PerceivedEntities.Single(item => item.EntityId == water.Id);

        agent.AddComponent(new MoveIntentComponent(new MoveIntent(agent.Id, before.Direction, 2, MovementMode.Walk)));
        world.RegisterSystem(new MovementResolutionSystem(terrain));
        world.Tick(TimeSpan.FromSeconds(1));

        var after = builder.BuildObservation(world, agent.Id).PerceivedEntities.Single(item => item.EntityId == water.Id);
        Assert.True(after.DistanceMeters < before.DistanceMeters);
        Assert.True(agent.GetComponent<EnergyNeedComponent>().CurrentEnergy < 100);
        Assert.True(agent.GetComponent<HydrationNeedComponent>().CurrentHydration < 100);
    }

    [Fact]
    public void MovementAndPerceptionLoop_MovesCloserToFood()
    {
        var world = CreateWorld(out var terrain);
        var agent = CreateObserver(world, new WorldPosition(1, 1, 0), range: 10, limit: 10);
        agent.AddComponent(new MovementCapabilityComponent(2, 1, 0, 0));
        var food = CreatePerceivedFood(world, new WorldPosition(5, 1, 0), "food");
        var builder = new AgentObservationBuilder();
        var before = builder.BuildObservation(world, agent.Id).PerceivedEntities.Single(item => item.EntityId == food.Id);

        agent.AddComponent(new MoveIntentComponent(new MoveIntent(agent.Id, before.Direction, 2, MovementMode.Walk)));
        world.RegisterSystem(new MovementResolutionSystem(terrain));
        world.Tick(TimeSpan.FromSeconds(1));

        var after = builder.BuildObservation(world, agent.Id).PerceivedEntities.Single(item => item.EntityId == food.Id);
        Assert.True(after.DistanceMeters < before.DistanceMeters);
    }

    private static WorldState CreateWorld(out ITerrainService terrain)
    {
        var registry = new DefinitionRegistry();
        registry.Register(new SoilDefinition(DefinitionId.From("soil.loam"), "Loam", 1, 0.5, 0.5, 0.5, 0.1));
        registry.Register(new SoilDefinition(DefinitionId.From("soil.mud"), "Mud", 2, 0.8, 0.2, 0.5, 0.2));
        registry.Register(new ConsumableDefinition(DefinitionId.From("consumable.grass"), "Grass", new[] { "grass" }, 1, 1, 0, 1));
        registry.Freeze();

        var grid = new TerrainGrid(10, 10, 1, DefinitionId.From("soil.loam"));
        terrain = new TerrainService(grid);
        return new WorldState(new SimulationTime(), new EventBus(), registry, NoOpTraceLogger.Instance);
    }

    private static Entity CreateMover(
        WorldState world,
        WorldPosition position,
        double speed = 10,
        double maxSlope = 10,
        double energy = 100,
        double hydration = 100,
        double energyCost = 0.1)
    {
        var entity = world.CreateEntity();
        entity.AddComponent(new PositionComponent(position));
        entity.AddComponent(new MovementCapabilityComponent(speed, maxSlope, energyCost, 0.1));
        entity.AddComponent(new EnergyNeedComponent(energy, 100, 0));
        entity.AddComponent(new HydrationNeedComponent(hydration, 100, 0));
        return entity;
    }

    private static Entity CreateObserver(WorldState world, WorldPosition position, double range, int limit)
    {
        var entity = world.CreateEntity();
        entity.AddComponent(new PositionComponent(position));
        entity.AddComponent(new PerceptionCapabilityComponent(range, 360, limit));
        entity.AddComponent(new PerceptionSignatureComponent(new[] { "observer" }));
        return entity;
    }

    private static Entity CreatePerceived(WorldState world, WorldPosition position, params string[] tags)
    {
        var entity = world.CreateEntity();
        entity.AddComponent(new PositionComponent(position));
        entity.AddComponent(new PerceptionSignatureComponent(tags));
        return entity;
    }

    private static Entity CreatePerceivedFood(WorldState world, WorldPosition position, params string[] tags)
    {
        var entity = CreatePerceived(world, position, tags);
        entity.AddComponent(new ConsumableResourceComponent(DefinitionReference<ConsumableDefinition>.From("consumable.grass"), 10, 10));
        return entity;
    }

    private static Entity CreatePerceivedWater(WorldState world, WorldPosition position, params string[] tags)
    {
        var entity = CreatePerceived(world, position, tags);
        entity.AddComponent(new WaterSourceComponent(10, 10, WaterQuality.Clean));
        return entity;
    }
}
