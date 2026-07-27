namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Identifies a future terrain chunk without imposing streaming or persistence yet.
/// </summary>
public readonly record struct TerrainChunkCoordinate(int X, int Y);
