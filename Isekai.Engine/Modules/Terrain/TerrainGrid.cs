using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Stores terrain as compact deterministic layers indexed by dense cell coordinates.
/// </summary>
public sealed class TerrainGrid : ITerrainGrid
{
    private static readonly TerrainCellCoordinate[] NeighborOffsets4 =
    [
        new(0, -1),
        new(1, 0),
        new(0, 1),
        new(-1, 0)
    ];

    private static readonly TerrainCellCoordinate[] NeighborOffsets8 =
    [
        new(0, -1),
        new(1, -1),
        new(1, 0),
        new(1, 1),
        new(0, 1),
        new(-1, 1),
        new(-1, 0),
        new(-1, -1)
    ];

    private readonly double[] _elevationLayer;
    private readonly DefinitionId[] _soilLayer;

    /// <summary>
    /// Creates a terrain grid with one elevation layer and one soil layer.
    /// </summary>
    public TerrainGrid(
        int widthInCells,
        int heightInCells,
        double cellSizeMeters,
        DefinitionId defaultSoilDefinitionId)
    {
        if (widthInCells <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(widthInCells), "Terrain width must be greater than zero.");
        }

        if (heightInCells <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(heightInCells), "Terrain height must be greater than zero.");
        }

        if (!double.IsFinite(cellSizeMeters) || cellSizeMeters <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cellSizeMeters), "Cell size must be a finite value greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(defaultSoilDefinitionId.Value))
        {
            throw new ArgumentException("Default soil definition id cannot be empty.", nameof(defaultSoilDefinitionId));
        }

        WidthInCells = widthInCells;
        HeightInCells = heightInCells;
        CellSizeMeters = cellSizeMeters;

        _elevationLayer = new double[widthInCells * heightInCells];
        _soilLayer = Enumerable.Repeat(defaultSoilDefinitionId, _elevationLayer.Length).ToArray();
    }

    /// <inheritdoc />
    public int WidthInCells { get; }

    /// <inheritdoc />
    public int HeightInCells { get; }

    /// <inheritdoc />
    public double CellSizeMeters { get; }

    /// <inheritdoc />
    public int CellCount => _elevationLayer.Length;

    /// <inheritdoc />
    public bool Contains(TerrainCellCoordinate coordinate)
    {
        return coordinate.X >= 0 &&
               coordinate.Y >= 0 &&
               coordinate.X < WidthInCells &&
               coordinate.Y < HeightInCells;
    }

    /// <inheritdoc />
    public TerrainCell GetCell(TerrainCellCoordinate coordinate)
    {
        var index = GetRequiredIndex(coordinate);
        return new TerrainCell(coordinate, _elevationLayer[index], _soilLayer[index]);
    }

    /// <inheritdoc />
    public bool TryGetCell(TerrainCellCoordinate coordinate, out TerrainCell cell)
    {
        if (!Contains(coordinate))
        {
            cell = default;
            return false;
        }

        cell = GetCell(coordinate);
        return true;
    }

    /// <inheritdoc />
    public TerrainCellCoordinate GetCoordinate(WorldPosition position)
    {
        if (!double.IsFinite(position.X) || !double.IsFinite(position.Y) || !double.IsFinite(position.Z))
        {
            throw new ArgumentOutOfRangeException(nameof(position), "World position must contain only finite values.");
        }

        if (position.X < 0 || position.Y < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Negative world positions are outside this terrain grid.");
        }

        var coordinate = new TerrainCellCoordinate(
            (int)Math.Floor(position.X / CellSizeMeters),
            (int)Math.Floor(position.Y / CellSizeMeters));

        if (!Contains(coordinate))
        {
            throw new ArgumentOutOfRangeException(nameof(position), "World position is outside this terrain grid.");
        }

        return coordinate;
    }

    /// <inheritdoc />
    public double GetElevation(TerrainCellCoordinate coordinate)
    {
        return _elevationLayer[GetRequiredIndex(coordinate)];
    }

    /// <inheritdoc />
    public void SetElevation(TerrainCellCoordinate coordinate, double elevationMeters)
    {
        if (!double.IsFinite(elevationMeters))
        {
            throw new ArgumentOutOfRangeException(nameof(elevationMeters), "Elevation must be a finite value.");
        }

        _elevationLayer[GetRequiredIndex(coordinate)] = elevationMeters;
    }

    /// <inheritdoc />
    public void SetSoil(TerrainCellCoordinate coordinate, DefinitionId soilDefinitionId)
    {
        if (string.IsNullOrWhiteSpace(soilDefinitionId.Value))
        {
            throw new ArgumentException("Soil definition id cannot be empty.", nameof(soilDefinitionId));
        }

        _soilLayer[GetRequiredIndex(coordinate)] = soilDefinitionId;
    }

    /// <inheritdoc />
    public IReadOnlyList<TerrainCellCoordinate> GetNeighbors4(TerrainCellCoordinate center)
    {
        EnsureContains(center);
        return GetNeighbors(center, NeighborOffsets4);
    }

    /// <inheritdoc />
    public IReadOnlyList<TerrainCellCoordinate> GetNeighbors8(TerrainCellCoordinate center)
    {
        EnsureContains(center);
        return GetNeighbors(center, NeighborOffsets8);
    }

    /// <inheritdoc />
    public double GetSlopeBetween(TerrainCellCoordinate from, TerrainCellCoordinate to)
    {
        EnsureContains(from);
        EnsureContains(to);

        if (from == to)
        {
            return 0;
        }

        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var horizontalDistanceMeters = Math.Sqrt((dx * dx) + (dy * dy)) * CellSizeMeters;
        var elevationDifferenceMeters = GetElevation(to) - GetElevation(from);

        return elevationDifferenceMeters / horizontalDistanceMeters;
    }

    /// <inheritdoc />
    public IEnumerable<TerrainCellCoordinate> EnumerateCells(TerrainRectangle region)
    {
        var minX = Math.Clamp(Math.Min(region.MinX, region.MaxX), 0, WidthInCells - 1);
        var maxX = Math.Clamp(Math.Max(region.MinX, region.MaxX), 0, WidthInCells - 1);
        var minY = Math.Clamp(Math.Min(region.MinY, region.MaxY), 0, HeightInCells - 1);
        var maxY = Math.Clamp(Math.Max(region.MinY, region.MaxY), 0, HeightInCells - 1);

        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                yield return new TerrainCellCoordinate(x, y);
            }
        }
    }

    /// <summary>
    /// Enumerates the valid cells covered by a centered terrain region in row-major order.
    /// </summary>
    public IEnumerable<TerrainCellCoordinate> EnumerateCells(TerrainRegion region)
    {
        return EnumerateCells(region.ToRectangle());
    }

    private IReadOnlyList<TerrainCellCoordinate> GetNeighbors(
        TerrainCellCoordinate center,
        IReadOnlyCollection<TerrainCellCoordinate> offsets)
    {
        var neighbors = new List<TerrainCellCoordinate>(offsets.Count);
        foreach (var offset in offsets)
        {
            var candidate = new TerrainCellCoordinate(center.X + offset.X, center.Y + offset.Y);
            if (Contains(candidate))
            {
                neighbors.Add(candidate);
            }
        }

        return neighbors;
    }

    private int GetRequiredIndex(TerrainCellCoordinate coordinate)
    {
        EnsureContains(coordinate);
        return coordinate.Y * WidthInCells + coordinate.X;
    }

    private void EnsureContains(TerrainCellCoordinate coordinate)
    {
        if (!Contains(coordinate))
        {
            throw new ArgumentOutOfRangeException(nameof(coordinate), "Terrain coordinate is outside this grid.");
        }
    }
}
