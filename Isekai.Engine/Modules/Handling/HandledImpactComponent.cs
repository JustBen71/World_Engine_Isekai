using Isekai.Engine.Core;
using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Handling;

/// <summary>
/// Describes a timed impact action performed by an actor with a held entity.
/// </summary>
public sealed record HandledImpactComponent(
    EntityId HeldEntityId,
    EntityId ContactEntityId,
    EntityId TargetEntityId,
    double Effort,
    int TicksRemaining,
    int TicksBetweenImpacts,
    int TicksUntilNextImpact = 0,
    string? TargetBodyPartId = null) : IComponent;
