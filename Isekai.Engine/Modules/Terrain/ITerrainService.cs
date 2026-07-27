namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Provides terrain access to systems without exposing global state.
/// </summary>
public interface ITerrainService
{
    /// <summary>
    /// Gets the terrain grid.
    /// </summary>
    ITerrainGrid Grid { get; }

    /// <summary>
    /// Gets the cell coordinate containing a continuous world position.
    /// </summary>
    TerrainCellCoordinate GetCellAt(WorldPosition position);
}
