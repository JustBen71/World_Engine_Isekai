namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Generates compact terrain grids from deterministic requests.
/// </summary>
public interface ITerrainGenerator
{
    /// <summary>
    /// Generates a terrain grid.
    /// </summary>
    TerrainGrid Generate(TerrainGenerationRequest request);
}
