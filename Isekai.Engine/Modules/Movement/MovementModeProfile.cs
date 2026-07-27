namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Describes how a movement mode changes speed and costs.
/// </summary>
public sealed record MovementModeProfile(
    double SpeedMultiplier,
    double EnergyCostMultiplier,
    double HydrationCostMultiplier,
    double MinimumMobilityRequired)
{
    /// <summary>
    /// Gets the default walk mode profile.
    /// </summary>
    public static MovementModeProfile Walk { get; } = new(1, 1, 1, 0.1);

    /// <summary>
    /// Gets the default run mode profile.
    /// </summary>
    public static MovementModeProfile Run { get; } = new(1.8, 2, 1.6, 0.35);
}
