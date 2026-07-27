using Isekai.Engine.Core;
using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Requests that one entity drinks from water stored at a terrain cell.
/// </summary>
public sealed record DrinkFromTerrainIntent(
    EntityId ConsumerEntityId,
    TerrainCellCoordinate SourceCoordinate,
    double RequestedVolumeLiters,
    double MaximumDistanceMeters = 1.5);
