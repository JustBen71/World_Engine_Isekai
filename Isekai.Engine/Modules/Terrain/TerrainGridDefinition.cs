using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Defines the dimensions and deterministic generation profile of a terrain grid.
/// </summary>
public sealed record TerrainGridDefinition(
    DefinitionId Id,
    int WidthInCells,
    int HeightInCells,
    double CellSizeMeters,
    DefinitionReference<SoilDefinition> DefaultSoil,
    TerrainGenerationKind GenerationKind,
    double MaxElevationMeters) : IDefinition;
