namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for generic movement capability.
/// </summary>
public sealed record SandboxMovementCapabilityData(
    double MaximumSpeedMetersPerSecond,
    double MaximumTraversableSlope,
    double BaseEnergyCostPerMeter,
    double BaseHydrationCostPerMeter);
