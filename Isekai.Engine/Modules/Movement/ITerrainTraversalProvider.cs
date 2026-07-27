using Isekai.Engine.Interfaces;
using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Provides traversal information for terrain cells.
/// </summary>
public interface ITerrainTraversalProvider
{
    /// <summary>
    /// Gets traversal information for a terrain coordinate.
    /// </summary>
    TraversalInfo GetTraversalInfo(IWorldState world, TerrainCellCoordinate coordinate);
}
