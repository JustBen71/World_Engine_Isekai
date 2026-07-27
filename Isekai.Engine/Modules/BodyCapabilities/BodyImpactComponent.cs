using Isekai.Engine.Core;
using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.BodyCapabilities;

/// <summary>
/// Describes a timed impact action performed with one authorized body contact surface.
/// </summary>
public sealed record BodyImpactComponent(
    string ContactRole,
    EntityId TargetEntityId,
    double Effort,
    int TicksRemaining,
    int TicksBetweenImpacts,
    int TicksUntilNextImpact = 0,
    string? TargetBodyPartId = null) : IComponent;
