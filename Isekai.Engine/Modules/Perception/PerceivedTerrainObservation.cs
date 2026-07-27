using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Perception;

/// <summary>
/// Snapshot of one perceived terrain resource.
/// </summary>
public sealed record PerceivedTerrainObservation(
    TerrainCellCoordinate Coordinate,
    double DistanceMeters,
    Movement.WorldVector Direction,
    IReadOnlyCollection<string> Tags,
    double Quantity);
