using Isekai.Engine.Core;
using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Stores one contamination exposure produced by consuming unsafe water.
/// </summary>
public sealed record ContaminationExposure(
    double BacterialExposure,
    double ChemicalExposure,
    double SalinityExposure,
    EntityId? SourceEntityId,
    TerrainCellCoordinate? SourceCoordinate,
    ulong Tick);
