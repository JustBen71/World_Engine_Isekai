namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Describes a deterministic terrain generation request.
/// </summary>
public sealed record TerrainGenerationRequest(TerrainGridDefinition Definition, int Seed = 0);
