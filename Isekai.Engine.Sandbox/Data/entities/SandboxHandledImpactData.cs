namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for a handled impact action in the sandbox.
/// </summary>
public sealed record SandboxHandledImpactData(
    string HeldEntity,
    string ContactEntity,
    string TargetEntity,
    double Effort,
    int TicksRemaining,
    int TicksBetweenImpacts,
    string? TargetBodyPart);
