using Isekai.Engine.Interfaces;
using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Uses terrain soil definitions and an optional impassable-cell set to resolve traversal.
/// </summary>
public sealed class TerrainTraversalProvider : ITerrainTraversalProvider
{
    private readonly ITerrainService _terrainService;
    private readonly HashSet<TerrainCellCoordinate> _impassableCells;

    /// <summary>
    /// Creates a traversal provider.
    /// </summary>
    public TerrainTraversalProvider(ITerrainService terrainService, IEnumerable<TerrainCellCoordinate>? impassableCells = null)
    {
        _terrainService = terrainService ?? throw new ArgumentNullException(nameof(terrainService));
        _impassableCells = impassableCells?.ToHashSet() ?? new HashSet<TerrainCellCoordinate>();
    }

    /// <inheritdoc />
    public TraversalInfo GetTraversalInfo(IWorldState world, TerrainCellCoordinate coordinate)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (!_terrainService.Grid.Contains(coordinate) || _impassableCells.Contains(coordinate))
        {
            return new TraversalInfo(false, 0, 1);
        }

        var cell = _terrainService.Grid.GetCell(coordinate);
        var cost = world.Definitions.TryGet<SoilDefinition>(cell.SoilDefinitionId, out var soil) && soil is not null
            ? soil.MovementCostMultiplier
            : 1;
        var safeCost = double.IsFinite(cost) && cost > 0 ? cost : 1;

        return new TraversalInfo(true, Math.Clamp(1 / safeCost, 0.05, 1), safeCost);
    }
}
