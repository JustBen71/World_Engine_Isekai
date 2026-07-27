using Isekai.Engine.Core;
using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Needs;
using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Resolves movement intents authoritatively using terrain, mobility and need costs.
/// </summary>
public sealed class MovementResolutionSystem : IWorldSystem
{
    private readonly ITerrainService _terrainService;
    private readonly IMobilityProvider _mobilityProvider;
    private readonly ITerrainTraversalProvider _traversalProvider;

    /// <summary>
    /// Creates a movement resolution system.
    /// </summary>
    public MovementResolutionSystem(
        ITerrainService terrainService,
        IMobilityProvider? mobilityProvider = null,
        ITerrainTraversalProvider? traversalProvider = null)
    {
        _terrainService = terrainService ?? throw new ArgumentNullException(nameof(terrainService));
        _mobilityProvider = mobilityProvider ?? new ComponentMobilityProvider();
        _traversalProvider = traversalProvider ?? new TerrainTraversalProvider(terrainService);
    }

    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var carrier in context.World.EntitiesWith<MoveIntentComponent>())
        {
            var intent = carrier.GetComponent<MoveIntentComponent>().Intent;
            var result = Resolve(context, intent);

            carrier.SetComponent(new MovementResultComponent(result));
            carrier.RemoveComponent<MoveIntentComponent>();
        }
    }

    private MovementResult Resolve(WorldSystemExecutionContext context, MoveIntent intent)
    {
        var zero = new WorldPosition(0, 0, 0);
        if (!intent.DesiredDirection.IsValidDirection ||
            !Enum.IsDefined(intent.Mode) ||
            !double.IsFinite(intent.DesiredDistanceMeters) ||
            intent.DesiredDistanceMeters <= 0)
        {
            return EmptyResult(intent.EntityId, zero, zero, intent.DesiredDistanceMeters, MovementOutcome.InvalidIntent);
        }

        if (!context.World.TryGetEntity(intent.EntityId, out var entity) || entity is null)
        {
            return EmptyResult(intent.EntityId, zero, zero, intent.DesiredDistanceMeters, MovementOutcome.MissingEntity);
        }

        if (!entity.TryGetComponent<PositionComponent>(out var position) || position is null)
        {
            return EmptyResult(intent.EntityId, zero, zero, intent.DesiredDistanceMeters, MovementOutcome.MissingPosition);
        }

        if (!entity.TryGetComponent<MovementCapabilityComponent>(out var capability) || capability is null ||
            !double.IsFinite(capability.MaximumSpeedMetersPerSecond) ||
            capability.MaximumSpeedMetersPerSecond <= 0)
        {
            return EmptyResult(intent.EntityId, position.Position, position.Position, intent.DesiredDistanceMeters, MovementOutcome.MissingCapability);
        }

        var mode = GetModeProfile(intent.Mode);
        var mobility = _mobilityProvider.GetMobilityMultiplier(context.World, entity);
        if (!double.IsFinite(mobility) || mobility <= 0)
        {
            return EmptyResult(intent.EntityId, position.Position, position.Position, intent.DesiredDistanceMeters, MovementOutcome.NoMobility);
        }

        mobility = Math.Clamp(mobility, 0, 1);
        if (mobility < mode.MinimumMobilityRequired)
        {
            return EmptyResult(intent.EntityId, position.Position, position.Position, intent.DesiredDistanceMeters, MovementOutcome.NoMobility);
        }

        var deltaSeconds = Math.Max(0, context.DeltaTime.TotalSeconds);
        var direction = intent.DesiredDirection.Normalize();
        var maxDistance = capability.MaximumSpeedMetersPerSecond * mode.SpeedMultiplier * deltaSeconds;
        var candidateDistance = Math.Min(intent.DesiredDistanceMeters, maxDistance);
        if (candidateDistance <= 0)
        {
            return EmptyResult(intent.EntityId, position.Position, position.Position, intent.DesiredDistanceMeters, MovementOutcome.PartialSuccess);
        }

        var candidateEnd = Add(position.Position, direction, candidateDistance);
        TerrainCellCoordinate startCell;
        TerrainCellCoordinate endCell;
        try
        {
            startCell = _terrainService.GetCellAt(position.Position);
            endCell = _terrainService.GetCellAt(candidateEnd);
        }
        catch (ArgumentOutOfRangeException)
        {
            return EmptyResult(intent.EntityId, position.Position, position.Position, intent.DesiredDistanceMeters, MovementOutcome.OutOfBounds);
        }

        var traversal = _traversalProvider.GetTraversalInfo(context.World, endCell);
        if (!traversal.IsTraversable)
        {
            return EmptyResult(intent.EntityId, position.Position, position.Position, intent.DesiredDistanceMeters, MovementOutcome.ImpassableTerrain);
        }

        var slope = Math.Abs(_terrainService.Grid.GetSlopeBetween(startCell, endCell));
        var maxSlope = double.IsFinite(capability.MaximumTraversableSlope) ? Math.Max(0, capability.MaximumTraversableSlope) : 0;
        if (slope > maxSlope)
        {
            return EmptyResult(intent.EntityId, position.Position, position.Position, intent.DesiredDistanceMeters, MovementOutcome.SlopeTooSteep);
        }

        var slopeMultiplier = Math.Clamp(1 - (slope / Math.Max(maxSlope, 0.000001)), 0.1, 1);
        var actualDistance = candidateDistance * traversal.SpeedMultiplier * slopeMultiplier * mobility;
        var costMultiplier = traversal.CostMultiplier * (1 + slope) / Math.Max(mobility, 0.000001);
        var energyCostPerMeter = Math.Max(0, capability.BaseEnergyCostPerMeter) * mode.EnergyCostMultiplier * costMultiplier;
        var hydrationCostPerMeter = Math.Max(0, capability.BaseHydrationCostPerMeter) * mode.HydrationCostMultiplier * costMultiplier;

        if (entity.TryGetComponent<EnergyNeedComponent>(out var energy) && energy is not null && energyCostPerMeter > 0)
        {
            if (energy.CurrentEnergy <= 0)
            {
                return EmptyResult(intent.EntityId, position.Position, position.Position, intent.DesiredDistanceMeters, MovementOutcome.InsufficientEnergy);
            }

            actualDistance = Math.Min(actualDistance, energy.CurrentEnergy / energyCostPerMeter);
        }

        actualDistance = Math.Clamp(actualDistance, 0, candidateDistance);
        var end = Add(position.Position, direction, actualDistance);
        try
        {
            _terrainService.GetCellAt(end);
        }
        catch (ArgumentOutOfRangeException)
        {
            return EmptyResult(intent.EntityId, position.Position, position.Position, intent.DesiredDistanceMeters, MovementOutcome.OutOfBounds);
        }

        var energyConsumed = ApplyEnergyCost(entity, actualDistance * energyCostPerMeter);
        var hydrationConsumed = ApplyHydrationCost(entity, actualDistance * hydrationCostPerMeter);
        entity.SetComponent(new PositionComponent(end));

        var outcome = actualDistance + 0.000001 >= intent.DesiredDistanceMeters
            ? MovementOutcome.Success
            : MovementOutcome.PartialSuccess;

        return new MovementResult(intent.EntityId, position.Position, end, intent.DesiredDistanceMeters, actualDistance, energyConsumed, hydrationConsumed, outcome);
    }

    private static MovementModeProfile GetModeProfile(MovementMode mode)
    {
        return mode switch
        {
            MovementMode.Walk => MovementModeProfile.Walk,
            MovementMode.Run => MovementModeProfile.Run,
            _ => MovementModeProfile.Walk
        };
    }

    private static WorldPosition Add(WorldPosition position, WorldVector direction, double distance)
    {
        return new WorldPosition(
            position.X + (direction.X * distance),
            position.Y + (direction.Y * distance),
            position.Z + (direction.Z * distance));
    }

    private static double ApplyEnergyCost(Entity entity, double cost)
    {
        if (!entity.TryGetComponent<EnergyNeedComponent>(out var need) || need is null)
        {
            return 0;
        }

        var consumed = Math.Min(Math.Max(0, need.CurrentEnergy), Math.Max(0, cost));
        entity.SetComponent(need with { CurrentEnergy = Math.Max(0, need.CurrentEnergy - consumed) });
        return consumed;
    }

    private static double ApplyHydrationCost(Entity entity, double cost)
    {
        if (!entity.TryGetComponent<HydrationNeedComponent>(out var need) || need is null)
        {
            return 0;
        }

        var consumed = Math.Min(Math.Max(0, need.CurrentHydration), Math.Max(0, cost));
        entity.SetComponent(need with { CurrentHydration = Math.Max(0, need.CurrentHydration - consumed) });
        return consumed;
    }

    private static MovementResult EmptyResult(
        EntityId entityId,
        WorldPosition start,
        WorldPosition end,
        double requestedDistance,
        MovementOutcome outcome)
    {
        return new MovementResult(entityId, start, end, requestedDistance, 0, 0, 0, outcome);
    }
}
