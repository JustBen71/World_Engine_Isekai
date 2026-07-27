using Isekai.Engine.Core;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Requests that one entity consumes a finite consumable resource entity.
/// </summary>
public sealed record ConsumeIntent(
    EntityId ConsumerEntityId,
    EntityId ResourceEntityId,
    double RequestedQuantity,
    double MaximumDistanceMeters = 1.5);
