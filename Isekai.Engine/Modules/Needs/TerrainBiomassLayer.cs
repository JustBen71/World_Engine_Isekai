using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Stores consumable biomass quantities by terrain cell without creating one entity per plant.
/// </summary>
public sealed class TerrainBiomassLayer
{
    private readonly Dictionary<TerrainCellCoordinate, double> _biomassByCell = new();

    /// <summary>
    /// Creates a biomass layer for one consumable definition.
    /// </summary>
    public TerrainBiomassLayer(DefinitionReference<ConsumableDefinition> consumableDefinition, double regenerationPerSecond = 0)
    {
        if (!double.IsFinite(regenerationPerSecond) || regenerationPerSecond < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(regenerationPerSecond), "Biomass regeneration must be finite and non-negative.");
        }

        ConsumableDefinition = consumableDefinition;
        RegenerationPerSecond = regenerationPerSecond;
    }

    /// <summary>
    /// Gets the consumable definition represented by this biomass layer.
    /// </summary>
    public DefinitionReference<ConsumableDefinition> ConsumableDefinition { get; }

    /// <summary>
    /// Gets passive biomass regeneration per second.
    /// </summary>
    public double RegenerationPerSecond { get; }

    /// <summary>
    /// Gets the biomass quantity at a cell.
    /// </summary>
    public double GetBiomass(TerrainCellCoordinate coordinate)
    {
        return _biomassByCell.TryGetValue(coordinate, out var value) ? value : 0;
    }

    /// <summary>
    /// Sets biomass at a cell.
    /// </summary>
    public void SetBiomass(TerrainCellCoordinate coordinate, double quantity)
    {
        if (!double.IsFinite(quantity) || quantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Biomass quantity must be finite and non-negative.");
        }

        _biomassByCell[coordinate] = quantity;
    }

    /// <summary>
    /// Consumes biomass at a cell and returns the actual consumed amount.
    /// </summary>
    public double ConsumeBiomass(TerrainCellCoordinate coordinate, double requestedQuantity)
    {
        if (!double.IsFinite(requestedQuantity) || requestedQuantity <= 0)
        {
            return 0;
        }

        var available = GetBiomass(coordinate);
        var consumed = Math.Min(available, requestedQuantity);
        _biomassByCell[coordinate] = Math.Max(0, available - consumed);
        return consumed;
    }
}
