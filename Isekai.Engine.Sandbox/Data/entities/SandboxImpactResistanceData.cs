namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for target impact resistance coefficients in the sandbox.
/// </summary>
public sealed record SandboxImpactResistanceData(
    double Hardness,
    double Toughness,
    double FractureResistance);
