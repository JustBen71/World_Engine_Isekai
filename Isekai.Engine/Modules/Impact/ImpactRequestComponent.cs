using Isekai.Engine.Core;
using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Impact;

/// <summary>
/// Requests that an impact is resolved during the next impact system execution.
/// </summary>
public sealed record ImpactRequestComponent(
    EntityId ContactEntityId,
    EntityId TargetEntityId,
    double Force,
    string? TargetBodyPartId = null) : IComponent;
