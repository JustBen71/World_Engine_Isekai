using Isekai.Engine.Modules.Movement;
using Isekai.Engine.Modules.Needs;
using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Perception;

/// <summary>
/// Exposes needs terrain resource layers as factual terrain observations.
/// </summary>
public sealed class NeedsTerrainResourcePerceptionProvider : IPerceptibleTerrainResourceProvider
{
    private readonly ITerrainGrid _terrainGrid;
    private readonly TerrainBiomassLayer? _biomassLayer;
    private readonly TerrainWaterLayer? _waterLayer;

    /// <summary>
    /// Creates a provider over optional needs terrain layers.
    /// </summary>
    public NeedsTerrainResourcePerceptionProvider(
        ITerrainGrid terrainGrid,
        TerrainBiomassLayer? biomassLayer,
        TerrainWaterLayer? waterLayer)
    {
        _terrainGrid = terrainGrid ?? throw new ArgumentNullException(nameof(terrainGrid));
        _biomassLayer = biomassLayer;
        _waterLayer = waterLayer;
    }

    /// <inheritdoc />
    public IReadOnlyList<PerceivedTerrainObservation> GetResources(WorldPosition observerPosition, double rangeMeters)
    {
        if (!double.IsFinite(rangeMeters) || rangeMeters <= 0)
        {
            return Array.Empty<PerceivedTerrainObservation>();
        }

        var results = new List<PerceivedTerrainObservation>();
        var radius = (int)Math.Ceiling(rangeMeters / _terrainGrid.CellSizeMeters);
        TerrainCellCoordinate center;
        try
        {
            center = _terrainGrid.GetCoordinate(observerPosition);
        }
        catch (ArgumentOutOfRangeException)
        {
            return Array.Empty<PerceivedTerrainObservation>();
        }

        foreach (var coordinate in _terrainGrid.EnumerateCells(new TerrainRegion(center, radius).ToRectangle()))
        {
            var cellCenter = new WorldPosition(
                (coordinate.X + 0.5) * _terrainGrid.CellSizeMeters,
                (coordinate.Y + 0.5) * _terrainGrid.CellSizeMeters,
                observerPosition.Z);
            var vector = new WorldVector(cellCenter.X - observerPosition.X, cellCenter.Y - observerPosition.Y, 0);
            var distance = vector.Length;
            if (distance > rangeMeters)
            {
                continue;
            }

            var direction = vector.Normalize();
            var biomass = _biomassLayer?.GetBiomass(coordinate) ?? 0;
            if (biomass > 0)
            {
                results.Add(new PerceivedTerrainObservation(coordinate, distance, direction, new[] { "terrain", "food", "biomass" }, biomass));
            }

            if (_waterLayer is not null &&
                _waterLayer.TryGetWater(coordinate, out var water) &&
                water.AvailableVolumeLiters > 0)
            {
                results.Add(new PerceivedTerrainObservation(coordinate, distance, direction, new[] { "terrain", "water" }, water.AvailableVolumeLiters));
            }
        }

        return results
            .OrderBy(result => result.DistanceMeters)
            .ThenBy(result => result.Coordinate.Y)
            .ThenBy(result => result.Coordinate.X)
            .ThenBy(result => string.Join(",", result.Tags), StringComparer.Ordinal)
            .ToArray();
    }
}
