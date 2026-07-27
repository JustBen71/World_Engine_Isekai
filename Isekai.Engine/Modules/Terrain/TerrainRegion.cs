namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Describes a square simulation region centered on one terrain cell.
/// </summary>
public readonly record struct TerrainRegion(TerrainCellCoordinate Center, int RadiusInCells)
{
    /// <summary>
    /// Converts the centered region to an inclusive rectangle.
    /// </summary>
    public TerrainRectangle ToRectangle()
    {
        var radius = Math.Max(0, RadiusInCells);
        return new TerrainRectangle(
            Center.X - radius,
            Center.Y - radius,
            Center.X + radius,
            Center.Y + radius);
    }
}
