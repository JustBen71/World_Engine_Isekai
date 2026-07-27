using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Stores drinkable water volumes by terrain cell without modeling full hydrology.
/// </summary>
public sealed class TerrainWaterLayer
{
    private readonly Dictionary<TerrainCellCoordinate, WaterCellResource> _waterByCell = new();

    /// <summary>
    /// Sets water resource data at a cell.
    /// </summary>
    public void SetWater(
        TerrainCellCoordinate coordinate,
        double availableVolumeLiters,
        WaterQuality quality,
        double refillLitersPerSecond = 0)
    {
        if (!double.IsFinite(availableVolumeLiters) || availableVolumeLiters < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(availableVolumeLiters), "Water volume must be finite and non-negative.");
        }

        if (!double.IsFinite(refillLitersPerSecond) || refillLitersPerSecond < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(refillLitersPerSecond), "Water refill rate must be finite and non-negative.");
        }

        _waterByCell[coordinate] = new WaterCellResource(availableVolumeLiters, quality, refillLitersPerSecond);
    }

    /// <summary>
    /// Returns true when a cell has a water resource.
    /// </summary>
    public bool TryGetWater(TerrainCellCoordinate coordinate, out WaterCellResource resource)
    {
        return _waterByCell.TryGetValue(coordinate, out resource);
    }

    /// <summary>
    /// Consumes water at a cell and returns the actual consumed volume.
    /// </summary>
    public double ConsumeWater(TerrainCellCoordinate coordinate, double requestedVolumeLiters)
    {
        if (!double.IsFinite(requestedVolumeLiters) || requestedVolumeLiters <= 0)
        {
            return 0;
        }

        if (!_waterByCell.TryGetValue(coordinate, out var resource))
        {
            return 0;
        }

        var consumed = Math.Min(resource.AvailableVolumeLiters, requestedVolumeLiters);
        _waterByCell[coordinate] = resource with
        {
            AvailableVolumeLiters = Math.Max(0, resource.AvailableVolumeLiters - consumed)
        };

        return consumed;
    }
}
