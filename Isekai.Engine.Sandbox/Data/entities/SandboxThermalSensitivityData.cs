namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for sandbox thermal sensitivity.
/// </summary>
public sealed record SandboxThermalSensitivityData(
    double ComfortableMinimum,
    double ComfortableMaximum);
