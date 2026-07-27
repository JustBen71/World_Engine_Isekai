namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model describing one body impact action.
/// </summary>
public sealed record SandboxBodyImpactData(
    string ContactRole,
    string TargetEntity,
    double Effort,
    int TicksRemaining,
    int TicksBetweenImpacts,
    string? TargetBodyPart);
