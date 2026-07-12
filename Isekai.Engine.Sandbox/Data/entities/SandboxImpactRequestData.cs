namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for one sandbox impact request.
/// </summary>
public sealed record SandboxImpactRequestData(
    string ContactEntity,
    string TargetEntity,
    double Force,
    string? TargetBodyPart);
