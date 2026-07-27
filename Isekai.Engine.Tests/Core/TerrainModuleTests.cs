using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Data.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Exceptions;
using Isekai.Engine.Interfaces;
using Isekai.Engine.Modules.Temperature;
using Isekai.Engine.Modules.Terrain;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for the compact terrain module.
/// </summary>
public sealed class TerrainModuleTests
{
    [Fact]
    public void TerrainGrid_CreatesGridWithExpectedDimensions()
    {
        var grid = CreateGrid(4, 3, 10);

        Assert.Equal(4, grid.WidthInCells);
        Assert.Equal(3, grid.HeightInCells);
        Assert.Equal(10, grid.CellSizeMeters);
        Assert.Equal(12, grid.CellCount);
    }

    [Fact]
    public void TerrainGrid_ReturnsCellsForValidCoordinates()
    {
        var grid = CreateGrid(3, 2, 1);
        grid.SetElevation(new TerrainCellCoordinate(2, 1), 4.5);
        grid.SetSoil(new TerrainCellCoordinate(2, 1), DefinitionId.From("soil.clay"));

        var cell = grid.GetCell(new TerrainCellCoordinate(2, 1));

        Assert.Equal(new TerrainCellCoordinate(2, 1), cell.Coordinate);
        Assert.Equal(4.5, cell.ElevationMeters);
        Assert.Equal(DefinitionId.From("soil.clay"), cell.SoilDefinitionId);
    }

    [Fact]
    public void TerrainGrid_HandlesInvalidCoordinatesCleanly()
    {
        var grid = CreateGrid(2, 2, 1);

        Assert.False(grid.TryGetCell(new TerrainCellCoordinate(-1, 0), out _));
        Assert.False(grid.TryGetCell(new TerrainCellCoordinate(2, 0), out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetCell(new TerrainCellCoordinate(0, 2)));
    }

    [Fact]
    public void TerrainGrid_ConvertsWorldPositionToCellCoordinate()
    {
        var grid = CreateGrid(5, 5, 10);

        Assert.Equal(new TerrainCellCoordinate(0, 0), grid.GetCoordinate(new WorldPosition(0, 0, 0)));
        Assert.Equal(new TerrainCellCoordinate(1, 2), grid.GetCoordinate(new WorldPosition(19, 24, 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetCoordinate(new WorldPosition(-1, 0, 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetCoordinate(new WorldPosition(50, 0, 0)));
    }

    [Fact]
    public void TerrainGrid_AllowsElevationMutationAndRejectsInvalidElevation()
    {
        var grid = CreateGrid(2, 2, 1);
        var coordinate = new TerrainCellCoordinate(1, 1);

        grid.SetElevation(coordinate, 12.25);

        Assert.Equal(12.25, grid.GetElevation(coordinate));
        Assert.Throws<ArgumentOutOfRangeException>(() => grid.SetElevation(coordinate, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => grid.SetElevation(coordinate, double.PositiveInfinity));
    }

    [Fact]
    public void SoilDefinitions_LoadAndValidateFromJson()
    {
        var registry = LoadTerrainDefinitions("""
            {
              "definitions": [
                {
                  "type": "terrain.soil",
                  "id": "soil.loam",
                  "displayName": "Loam",
                  "movementCostMultiplier": 1.0,
                  "waterRetention": 0.75,
                  "drainage": 0.55,
                  "fertility": 0.8,
                  "hardness": 0.35
                }
              ]
            }
            """);

        var soil = registry.Get<SoilDefinition>(DefinitionId.From("soil.loam"));

        Assert.Equal("Loam", soil.DisplayName);
        Assert.Equal(0.8, soil.Fertility);
    }

    [Fact]
    public void TerrainGridDefinition_RejectsUnknownSoilReference()
    {
        Assert.Throws<DefinitionValidationException>(() => LoadTerrainDefinitions("""
            {
              "definitions": [
                {
                  "type": "terrain.grid",
                  "id": "terrain.bad",
                  "widthInCells": 2,
                  "heightInCells": 2,
                  "cellSizeMeters": 1.0,
                  "defaultSoil": "soil.missing",
                  "generationKind": "Flat",
                  "maxElevationMeters": 0
                }
              ]
            }
            """));
    }

    [Fact]
    public void SoilDefinitionValidator_RejectsInvalidRatios()
    {
        Assert.Throws<InvalidDefinitionDataException>(() => LoadTerrainDefinitions("""
            {
              "definitions": [
                {
                  "type": "terrain.soil",
                  "id": "soil.bad",
                  "displayName": "Bad",
                  "movementCostMultiplier": 1.0,
                  "waterRetention": 1.5,
                  "drainage": 0.5,
                  "fertility": 0.5,
                  "hardness": 0.1
                }
              ]
            }
            """));
    }

    [Fact]
    public void TerrainGrid_ReturnsCardinalNeighbors()
    {
        var grid = CreateGrid(3, 3, 1);

        Assert.Equal(
            new[] { new TerrainCellCoordinate(1, 0), new(2, 1), new(1, 2), new(0, 1) },
            grid.GetNeighbors4(new TerrainCellCoordinate(1, 1)));
        Assert.Equal(3, grid.GetNeighbors4(new TerrainCellCoordinate(1, 0)).Count);
        Assert.Equal(2, grid.GetNeighbors4(new TerrainCellCoordinate(0, 0)).Count);
    }

    [Fact]
    public void TerrainGrid_ReturnsDiagonalNeighbors()
    {
        var grid = CreateGrid(3, 3, 1);

        Assert.Equal(8, grid.GetNeighbors8(new TerrainCellCoordinate(1, 1)).Count);
        Assert.Equal(
            new[] { new TerrainCellCoordinate(1, 0), new(1, 1), new(0, 1) },
            grid.GetNeighbors8(new TerrainCellCoordinate(0, 0)));
    }

    [Fact]
    public void TerrainGrid_ReturnsNeighborsInDeterministicOrder()
    {
        var grid = CreateGrid(3, 3, 1);
        var center = new TerrainCellCoordinate(1, 1);

        Assert.Equal(grid.GetNeighbors8(center), grid.GetNeighbors8(center));
    }

    [Fact]
    public void TerrainGrid_EnumeratesRegionWithClipping()
    {
        var grid = CreateGrid(4, 4, 1);
        var cells = grid.EnumerateCells(new TerrainRegion(new TerrainCellCoordinate(0, 0), 1)).ToArray();

        Assert.Equal(
            new[] { new TerrainCellCoordinate(0, 0), new(1, 0), new(0, 1), new(1, 1) },
            cells);
    }

    [Fact]
    public void TerrainGrid_ComputesSlopeAsElevationRatio()
    {
        var grid = CreateGrid(2, 1, 10);
        grid.SetElevation(new TerrainCellCoordinate(0, 0), 5);
        grid.SetElevation(new TerrainCellCoordinate(1, 0), 15);

        var slope = grid.GetSlopeBetween(new TerrainCellCoordinate(0, 0), new TerrainCellCoordinate(1, 0));

        Assert.Equal(1, slope);
    }

    [Fact]
    public void SimpleTerrainGenerator_IsDeterministic()
    {
        var definition = CreateGridDefinition(TerrainGenerationKind.CentralHill);
        var generator = new SimpleTerrainGenerator();

        var first = generator.Generate(new TerrainGenerationRequest(definition, Seed: 42));
        var second = generator.Generate(new TerrainGenerationRequest(definition, Seed: 42));

        foreach (var coordinate in first.EnumerateCells(new TerrainRectangle(0, 0, 3, 3)))
        {
            Assert.Equal(first.GetCell(coordinate), second.GetCell(coordinate));
        }
    }

    [Fact]
    public void TerrainService_FindsCellForPositionedEntity()
    {
        var service = new TerrainService(CreateGrid(5, 5, 10));
        var world = new WorldState();
        var entity = world.CreateEntity();
        entity.AddComponent(new PositionComponent(new WorldPosition(19, 24, 0)));

        var coordinate = service.GetCellAt(entity.GetComponent<PositionComponent>().Position);

        Assert.Equal(new TerrainCellCoordinate(1, 2), coordinate);
    }

    [Fact]
    public void TemperatureExchangeSystem_CanUseTerrainBasedAmbientProvider()
    {
        var grid = CreateGrid(3, 1, 1);
        var service = new TerrainService(grid);
        var world = new WorldState();
        var cold = world.CreateEntity();
        var hot = world.CreateEntity();

        cold.AddComponent(new PositionComponent(new WorldPosition(0.2, 0, 0)));
        cold.AddComponent(new TemperatureComponent(20));
        hot.AddComponent(new PositionComponent(new WorldPosition(2.2, 0, 0)));
        hot.AddComponent(new TemperatureComponent(20));

        world.RegisterSystem(new TemperatureExchangeSystem(new TerrainAmbientTemperatureProvider(service)));
        world.Tick(TimeSpan.FromSeconds(1));

        Assert.True(cold.GetComponent<TemperatureComponent>().Celsius < 20);
        Assert.True(hot.GetComponent<TemperatureComponent>().Celsius > 20);
    }

    [Fact]
    public void ActiveSimulationRegionRegistry_AddsRemovesAndReturnsRegionsInOrder()
    {
        var registry = new ActiveSimulationRegionRegistry();
        var first = new ActiveSimulationRegion("first", new TerrainRectangle(0, 0, 1, 1));
        var second = new ActiveSimulationRegion("second", new TerrainRectangle(2, 2, 3, 3));

        registry.AddRegion(first);
        registry.AddRegion(second);

        Assert.Equal(new[] { first, second }, registry.GetActiveRegions());
        Assert.True(registry.RemoveRegion("first"));
        Assert.Equal(new[] { second }, registry.GetActiveRegions());
    }

    [Fact]
    public void TerrainModule_RegistersDefinitionTypesAndNoRuntimeSystem()
    {
        var world = new WorldState();
        var module = new TerrainModule();
        var typeRegistry = new DefinitionTypeRegistry();

        module.RegisterDefinitionTypes(typeRegistry);
        module.RegisterSystems(world);

        Assert.Contains("terrain.soil", typeRegistry.TypeNames);
        Assert.Contains("terrain.grid", typeRegistry.TypeNames);
        Assert.Empty(world.Systems);
    }

    private static TerrainGrid CreateGrid(int width, int height, double cellSizeMeters)
    {
        return new TerrainGrid(width, height, cellSizeMeters, DefinitionId.From("soil.loam"));
    }

    private static TerrainGridDefinition CreateGridDefinition(TerrainGenerationKind generationKind)
    {
        return new TerrainGridDefinition(
            DefinitionId.From("terrain.test"),
            WidthInCells: 4,
            HeightInCells: 4,
            CellSizeMeters: 1,
            DefaultSoil: DefinitionReference<SoilDefinition>.From("soil.loam"),
            GenerationKind: generationKind,
            MaxElevationMeters: 10);
    }

    private static DefinitionRegistry LoadTerrainDefinitions(string json)
    {
        var module = new TerrainModule();
        var typeRegistry = new DefinitionTypeRegistry(NoOpTraceLogger.Instance);
        module.RegisterDefinitionTypes(typeRegistry);

        return new DefinitionLoadPipeline(
                new JsonDefinitionLoader(typeRegistry, NoOpTraceLogger.Instance),
                module.CreateDefinitionValidators(),
                NoOpTraceLogger.Instance)
            .LoadJson(json);
    }

    private sealed class TerrainAmbientTemperatureProvider : IAmbientTemperatureProvider
    {
        private readonly ITerrainService _terrainService;

        public TerrainAmbientTemperatureProvider(ITerrainService terrainService)
        {
            _terrainService = terrainService;
        }

        public double GetAmbientTemperatureCelsius(IWorldState world, Entity entity)
        {
            var position = entity.GetComponent<PositionComponent>().Position;
            var coordinate = _terrainService.GetCellAt(position);

            return coordinate.X == 0 ? 0 : 40;
        }
    }
}
