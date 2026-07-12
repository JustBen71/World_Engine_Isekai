using Isekai.Engine.Core;
using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Impact;

/// <summary>
/// Stores the last resolved impact result on the source entity.
/// </summary>
public sealed record ImpactResultComponent(
    EntityId ContactEntityId,
    EntityId TargetEntityId,
    string? TargetBodyPartId,
    double Force,
    double ImpactRatio,
    ImpactOutcome Outcome) : IComponent;
