using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Perception;

/// <summary>
/// Provides perceived terrain resources without copying terrain resource layers.
/// </summary>
public interface IPerceptibleTerrainResourceProvider
{
    /// <summary>
    /// Gets terrain resource observations around a position.
    /// </summary>
    IReadOnlyList<PerceivedTerrainObservation> GetResources(
        WorldPosition observerPosition,
        double rangeMeters);
}
