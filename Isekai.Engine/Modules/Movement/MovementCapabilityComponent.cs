using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Stores generic movement capacity and movement need costs.
/// </summary>
public sealed record MovementCapabilityComponent(
    double MaximumSpeedMetersPerSecond,
    double MaximumTraversableSlope,
    double BaseEnergyCostPerMeter,
    double BaseHydrationCostPerMeter) : IComponent;
