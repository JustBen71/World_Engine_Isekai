using Isekai.Engine.Core;

namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Requests local movement in a direction for a desired distance.
/// </summary>
public sealed record MoveIntent(
    EntityId EntityId,
    WorldVector DesiredDirection,
    double DesiredDistanceMeters,
    MovementMode Mode);
