namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for entity-specific natural recovery in the sandbox.
/// </summary>
public sealed record SandboxNaturalRecoveryData(
    double TissueRecoveryPerSecond,
    double BleedingRecoveryPerSecond);
