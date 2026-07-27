namespace Isekai.Engine.Modules.Perception;

/// <summary>
/// Snapshot of internal state exposed to controllers without exposing components.
/// </summary>
public sealed record InternalObservation(
    double EnergyNormalized,
    double HydrationNormalized,
    double MobilityNormalized,
    double ThermalComfortNormalized,
    bool IsAlive);
