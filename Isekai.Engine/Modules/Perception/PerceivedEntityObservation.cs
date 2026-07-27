using Isekai.Engine.Core;

namespace Isekai.Engine.Modules.Perception;

/// <summary>
/// Snapshot of one perceived entity.
/// </summary>
public sealed record PerceivedEntityObservation(
    EntityId EntityId,
    double DistanceMeters,
    Movement.WorldVector Direction,
    IReadOnlyCollection<string> Tags,
    double? AvailableFoodQuantity,
    double? AvailableWaterLiters);
