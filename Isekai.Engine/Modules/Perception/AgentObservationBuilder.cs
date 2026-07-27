using Isekai.Engine.Core;
using Isekai.Engine.Interfaces;
using Isekai.Engine.Modules.Movement;
using Isekai.Engine.Modules.Needs;
using Isekai.Engine.Modules.Temperature;
using Isekai.Engine.Modules.Terrain;
using Isekai.Engine.Modules.Vitals;

namespace Isekai.Engine.Modules.Perception;

/// <summary>
/// Builds factual observation snapshots on demand for future controllers.
/// </summary>
public sealed class AgentObservationBuilder
{
    private readonly IMobilityProvider _mobilityProvider;
    private readonly IPerceptibleTerrainResourceProvider? _terrainResourceProvider;

    /// <summary>
    /// Creates an observation builder.
    /// </summary>
    public AgentObservationBuilder(
        IMobilityProvider? mobilityProvider = null,
        IPerceptibleTerrainResourceProvider? terrainResourceProvider = null)
    {
        _mobilityProvider = mobilityProvider ?? new ComponentMobilityProvider();
        _terrainResourceProvider = terrainResourceProvider;
    }

    /// <summary>
    /// Builds an observation for one entity.
    /// </summary>
    public AgentObservation BuildObservation(IWorldState world, EntityId observerEntityId)
    {
        ArgumentNullException.ThrowIfNull(world);

        var observer = world.GetEntity(observerEntityId);
        var position = observer.GetComponent<PositionComponent>().Position;
        var capability = observer.GetComponent<PerceptionCapabilityComponent>();
        var range = double.IsFinite(capability.MaximumRangeMeters) ? Math.Max(0, capability.MaximumRangeMeters) : 0;
        var limit = capability.MaximumPerceivedEntities <= 0 ? int.MaxValue : capability.MaximumPerceivedEntities;

        var perceivedEntities = world.Entities
            .Where(entity => entity.Id != observer.Id)
            .Select(entity => TryBuildEntityObservation(entity, position, range))
            .Where(observation => observation is not null)
            .Cast<PerceivedEntityObservation>()
            .OrderBy(observation => observation.DistanceMeters)
            .ThenBy(observation => observation.EntityId.Value)
            .Take(limit)
            .ToArray();

        var perceivedTerrain = _terrainResourceProvider?.GetResources(position, range) ??
                               Array.Empty<PerceivedTerrainObservation>();

        return new AgentObservation(
            observer.Id,
            position,
            BuildInternalObservation(world, observer),
            perceivedEntities,
            perceivedTerrain);
    }

    private static PerceivedEntityObservation? TryBuildEntityObservation(
        Entity entity,
        WorldPosition observerPosition,
        double rangeMeters)
    {
        if (!entity.TryGetComponent<PositionComponent>(out var position) || position is null ||
            !entity.TryGetComponent<PerceptionSignatureComponent>(out var signature) || signature is null)
        {
            return null;
        }

        var vector = new WorldVector(
            position.Position.X - observerPosition.X,
            position.Position.Y - observerPosition.Y,
            position.Position.Z - observerPosition.Z);
        var distance = vector.Length;
        if (distance > rangeMeters)
        {
            return null;
        }

        var food = entity.TryGetComponent<ConsumableResourceComponent>(out var consumable) && consumable is not null
            ? Math.Max(0, consumable.CurrentQuantity)
            : (double?)null;
        var water = entity.TryGetComponent<WaterSourceComponent>(out var waterSource) && waterSource is not null
            ? Math.Max(0, waterSource.CurrentVolumeLiters)
            : (double?)null;

        return new PerceivedEntityObservation(
            entity.Id,
            distance,
            vector.Normalize(),
            signature.Tags.ToArray(),
            food,
            water);
    }

    private InternalObservation BuildInternalObservation(IWorldState world, Entity observer)
    {
        var energy = observer.TryGetComponent<EnergyNeedComponent>(out var energyNeed) && energyNeed is not null
            ? Normalize(energyNeed.CurrentEnergy, energyNeed.MaximumEnergy)
            : 1;
        var hydration = observer.TryGetComponent<HydrationNeedComponent>(out var hydrationNeed) && hydrationNeed is not null
            ? Normalize(hydrationNeed.CurrentHydration, hydrationNeed.MaximumHydration)
            : 1;
        var comfort = observer.TryGetComponent<ThermalComfortComponent>(out var thermalComfort) && thermalComfort is not null
            ? Math.Clamp(thermalComfort.Comfort, 0, 1)
            : 1;
        var alive = !observer.TryGetComponent<VitalStateComponent>(out var vital) || vital is null || vital.IsAlive;
        var mobility = Math.Clamp(_mobilityProvider.GetMobilityMultiplier(world, observer), 0, 1);

        return new InternalObservation(energy, hydration, mobility, comfort, alive);
    }

    private static double Normalize(double current, double maximum)
    {
        if (!double.IsFinite(current) || !double.IsFinite(maximum) || maximum <= 0)
        {
            return 0;
        }

        return Math.Clamp(current / maximum, 0, 1);
    }
}
