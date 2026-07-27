namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Generates simple deterministic terrain shapes for tests and sandbox scenarios.
/// </summary>
public sealed class SimpleTerrainGenerator : ITerrainGenerator
{
    /// <inheritdoc />
    public TerrainGrid Generate(TerrainGenerationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Definition);

        var definition = request.Definition;
        var grid = new TerrainGrid(
            definition.WidthInCells,
            definition.HeightInCells,
            definition.CellSizeMeters,
            definition.DefaultSoil.Id);

        for (var y = 0; y < grid.HeightInCells; y++)
        {
            for (var x = 0; x < grid.WidthInCells; x++)
            {
                var coordinate = new TerrainCellCoordinate(x, y);
                grid.SetElevation(coordinate, ResolveElevation(definition, x, y));
            }
        }

        return grid;
    }

    private static double ResolveElevation(TerrainGridDefinition definition, int x, int y)
    {
        return definition.GenerationKind switch
        {
            TerrainGenerationKind.Flat => 0,
            TerrainGenerationKind.Slope => ResolveSlopeElevation(definition, x),
            TerrainGenerationKind.CentralHill => ResolveCentralHillElevation(definition, x, y),
            _ => 0
        };
    }

    private static double ResolveSlopeElevation(TerrainGridDefinition definition, int x)
    {
        if (definition.WidthInCells <= 1)
        {
            return definition.MaxElevationMeters;
        }

        return definition.MaxElevationMeters * x / (definition.WidthInCells - 1);
    }

    private static double ResolveCentralHillElevation(TerrainGridDefinition definition, int x, int y)
    {
        var centerX = (definition.WidthInCells - 1) / 2.0;
        var centerY = (definition.HeightInCells - 1) / 2.0;
        var maxDistance = Math.Max(1, Math.Sqrt((centerX * centerX) + (centerY * centerY)));
        var distance = Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2));
        var normalized = Math.Clamp(1 - (distance / maxDistance), 0, 1);

        return definition.MaxElevationMeters * normalized;
    }
}
