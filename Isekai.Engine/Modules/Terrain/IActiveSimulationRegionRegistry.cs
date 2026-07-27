namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Stores the terrain regions currently relevant to simulation.
/// </summary>
public interface IActiveSimulationRegionRegistry
{
    /// <summary>
    /// Adds or replaces an active simulation region.
    /// </summary>
    void AddRegion(ActiveSimulationRegion region);

    /// <summary>
    /// Removes an active simulation region by identifier.
    /// </summary>
    bool RemoveRegion(string id);

    /// <summary>
    /// Gets all active simulation regions in deterministic insertion order.
    /// </summary>
    IReadOnlyCollection<ActiveSimulationRegion> GetActiveRegions();
}
