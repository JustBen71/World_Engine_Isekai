namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Describes a rectangular terrain region using inclusive cell bounds.
/// </summary>
public readonly record struct TerrainRectangle(int MinX, int MinY, int MaxX, int MaxY);
