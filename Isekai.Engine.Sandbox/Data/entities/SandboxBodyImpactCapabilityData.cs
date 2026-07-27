namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model describing body impact force capability.
/// </summary>
public sealed record SandboxBodyImpactCapabilityData(
    double MaxForce,
    double ImpactForce,
    double Precision);
