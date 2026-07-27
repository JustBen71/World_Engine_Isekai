using Isekai.Engine.Core;
using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Perception;

/// <summary>
/// Complete factual observation snapshot for one agent.
/// </summary>
public sealed record AgentObservation(
    EntityId ObserverEntityId,
    WorldPosition ObserverPosition,
    InternalObservation InternalState,
    IReadOnlyList<PerceivedEntityObservation> PerceivedEntities,
    IReadOnlyList<PerceivedTerrainObservation> PerceivedTerrain);
