namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Describes a rectangular chunk of terrain cells for future loading or simulation partitioning.
/// </summary>
public sealed class TerrainChunk
{
    /// <summary>
    /// Creates a terrain chunk descriptor.
    /// </summary>
    public TerrainChunk(
        TerrainChunkCoordinate coordinate,
        TerrainCellCoordinate origin,
        int widthInCells,
        int heightInCells)
    {
        if (widthInCells <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(widthInCells), "Chunk width must be greater than zero.");
        }

        if (heightInCells <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(heightInCells), "Chunk height must be greater than zero.");
        }

        Coordinate = coordinate;
        Origin = origin;
        WidthInCells = widthInCells;
        HeightInCells = heightInCells;
    }

    /// <summary>
    /// Gets the chunk coordinate.
    /// </summary>
    public TerrainChunkCoordinate Coordinate { get; }

    /// <summary>
    /// Gets the first cell coordinate covered by this chunk.
    /// </summary>
    public TerrainCellCoordinate Origin { get; }

    /// <summary>
    /// Gets the chunk width in cells.
    /// </summary>
    public int WidthInCells { get; }

    /// <summary>
    /// Gets the chunk height in cells.
    /// </summary>
    public int HeightInCells { get; }
}
