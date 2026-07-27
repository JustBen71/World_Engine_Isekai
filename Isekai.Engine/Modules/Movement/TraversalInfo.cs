namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Describes whether and how a terrain cell can be traversed.
/// </summary>
public sealed record TraversalInfo(bool IsTraversable, double SpeedMultiplier, double CostMultiplier);
