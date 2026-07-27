namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Default terrain service backed by one compact grid.
/// </summary>
public sealed class TerrainService : ITerrainService
{
    /// <summary>
    /// Creates a terrain service.
    /// </summary>
    public TerrainService(ITerrainGrid grid)
    {
        Grid = grid ?? throw new ArgumentNullException(nameof(grid));
    }

    /// <inheritdoc />
    public ITerrainGrid Grid { get; }

    /// <inheritdoc />
    public TerrainCellCoordinate GetCellAt(WorldPosition position)
    {
        return Grid.GetCoordinate(position);
    }
}
