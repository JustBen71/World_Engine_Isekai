using Isekai.Engine.Core;
using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Reports the authoritative result of one movement intent.
/// </summary>
public sealed record MovementResult(
    EntityId EntityId,
    WorldPosition StartPosition,
    WorldPosition EndPosition,
    double RequestedDistanceMeters,
    double ActualDistanceMeters,
    double EnergyConsumed,
    double HydrationConsumed,
    MovementOutcome Outcome);
