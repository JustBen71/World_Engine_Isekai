using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Perception;

/// <summary>
/// Stores simplified spatial perception capabilities.
/// </summary>
public sealed record PerceptionCapabilityComponent(
    double MaximumRangeMeters,
    double FieldOfViewDegrees,
    int MaximumPerceivedEntities) : IComponent;
