namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for entity energy and hydration needs.
/// </summary>
public sealed record SandboxNeedsData(
    double CurrentEnergy,
    double MaximumEnergy,
    double EnergyConsumptionPerSecond,
    double CurrentHydration,
    double MaximumHydration,
    double HydrationConsumptionPerSecond);
