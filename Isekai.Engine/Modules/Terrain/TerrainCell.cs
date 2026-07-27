using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Exposes the structural terrain data stored at one cell coordinate.
/// </summary>
public readonly record struct TerrainCell(
    TerrainCellCoordinate Coordinate,
    double ElevationMeters,
    DefinitionId SoilDefinitionId);
