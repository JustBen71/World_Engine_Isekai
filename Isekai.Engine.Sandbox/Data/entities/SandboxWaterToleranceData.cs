namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for water contamination tolerance.
/// </summary>
public sealed record SandboxWaterToleranceData(
    double BacterialTolerance,
    double ChemicalTolerance,
    double SalinityTolerance);
