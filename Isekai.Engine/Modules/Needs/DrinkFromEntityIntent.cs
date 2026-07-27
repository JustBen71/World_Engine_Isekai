using Isekai.Engine.Core;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Requests that one entity drinks from a water source entity.
/// </summary>
public sealed record DrinkFromEntityIntent(
    EntityId ConsumerEntityId,
    EntityId SourceEntityId,
    double RequestedVolumeLiters,
    double MaximumDistanceMeters = 1.5);
