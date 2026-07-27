namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Represents a terrain region that current simulation code considers important.
/// </summary>
public sealed record ActiveSimulationRegion(string Id, TerrainRectangle Bounds);
