using Isekai.Engine.Core;

namespace Isekai.Engine.Modules.Impact;

/// <summary>
/// Event published when an impact has been resolved.
/// </summary>
public sealed record ImpactResolvedEvent(
    EntityId SourceEntityId,
    EntityId ContactEntityId,
    EntityId TargetEntityId,
    string? TargetBodyPartId,
    double Force,
    double ImpactRatio,
    ImpactOutcome Outcome);
