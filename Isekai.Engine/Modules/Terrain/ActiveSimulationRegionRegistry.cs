namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// In-memory registry of terrain regions that are relevant to current simulation.
/// </summary>
public sealed class ActiveSimulationRegionRegistry : IActiveSimulationRegionRegistry
{
    private readonly List<ActiveSimulationRegion> _regions = new();

    /// <inheritdoc />
    public void AddRegion(ActiveSimulationRegion region)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(region.Id);

        RemoveRegion(region.Id);
        _regions.Add(region);
    }

    /// <inheritdoc />
    public bool RemoveRegion(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var index = _regions.FindIndex(region => string.Equals(region.Id, id, StringComparison.Ordinal));
        if (index < 0)
        {
            return false;
        }

        _regions.RemoveAt(index);
        return true;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ActiveSimulationRegion> GetActiveRegions()
    {
        return _regions.ToArray();
    }
}
