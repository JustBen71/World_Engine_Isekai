using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Provides deterministic access to a compact terrain grid.
/// </summary>
public interface ITerrainGrid
{
    /// <summary>
    /// Gets the grid width in cells.
    /// </summary>
    int WidthInCells { get; }

    /// <summary>
    /// Gets the grid height in cells.
    /// </summary>
    int HeightInCells { get; }

    /// <summary>
    /// Gets the physical size of one cell in meters.
    /// </summary>
    double CellSizeMeters { get; }

    /// <summary>
    /// Gets the total number of cells.
    /// </summary>
    int CellCount { get; }

    /// <summary>
    /// Returns true when a coordinate is inside the grid.
    /// </summary>
    bool Contains(TerrainCellCoordinate coordinate);

    /// <summary>
    /// Gets a copy of a terrain cell.
    /// </summary>
    TerrainCell GetCell(TerrainCellCoordinate coordinate);

    /// <summary>
    /// Tries to get a copy of a terrain cell.
    /// </summary>
    bool TryGetCell(TerrainCellCoordinate coordinate, out TerrainCell cell);

    /// <summary>
    /// Converts a continuous world position to the containing cell coordinate.
    /// </summary>
    TerrainCellCoordinate GetCoordinate(WorldPosition position);

    /// <summary>
    /// Gets the altitude at a cell coordinate in meters.
    /// </summary>
    double GetElevation(TerrainCellCoordinate coordinate);

    /// <summary>
    /// Sets the altitude at a cell coordinate in meters.
    /// </summary>
    void SetElevation(TerrainCellCoordinate coordinate, double elevationMeters);

    /// <summary>
    /// Sets the soil definition id at a cell coordinate.
    /// </summary>
    void SetSoil(TerrainCellCoordinate coordinate, DefinitionId soilDefinitionId);

    /// <summary>
    /// Gets valid cardinal neighbors in North, East, South, West order.
    /// </summary>
    IReadOnlyList<TerrainCellCoordinate> GetNeighbors4(TerrainCellCoordinate center);

    /// <summary>
    /// Gets valid eight-way neighbors in North, North-East, East, South-East, South, South-West, West, North-West order.
    /// </summary>
    IReadOnlyList<TerrainCellCoordinate> GetNeighbors8(TerrainCellCoordinate center);

    /// <summary>
    /// Gets the elevation ratio between two cells: elevation difference divided by horizontal distance.
    /// </summary>
    double GetSlopeBetween(TerrainCellCoordinate from, TerrainCellCoordinate to);

    /// <summary>
    /// Enumerates the valid cells covered by a rectangular terrain region in row-major order.
    /// </summary>
    IEnumerable<TerrainCellCoordinate> EnumerateCells(TerrainRectangle region);
}
