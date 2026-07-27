namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Describes water stored at one terrain cell.
/// </summary>
public readonly record struct WaterCellResource(
    double AvailableVolumeLiters,
    WaterQuality Quality,
    double RefillLitersPerSecond);
