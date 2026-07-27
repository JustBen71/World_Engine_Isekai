using Isekai.Engine.Core;
using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Reports the resolution of one drinking intent.
/// </summary>
public sealed record DrinkActionResult(
    EntityId ConsumerEntityId,
    EntityId? SourceEntityId,
    TerrainCellCoordinate? SourceCoordinate,
    double RequestedVolumeLiters,
    double ConsumedVolumeLiters,
    double HydrationReceived,
    double BacterialExposure,
    double ChemicalExposure,
    double SalinityExposure,
    DrinkOutcome Outcome);
