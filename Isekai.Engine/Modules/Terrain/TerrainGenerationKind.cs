namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Lists the deterministic terrain generation shapes supported by the first terrain module.
/// </summary>
public enum TerrainGenerationKind
{
    /// <summary>
    /// All cells use zero elevation.
    /// </summary>
    Flat,

    /// <summary>
    /// Elevation increases progressively from west to east.
    /// </summary>
    Slope,

    /// <summary>
    /// Elevation forms a simple central hill.
    /// </summary>
    CentralHill
}
