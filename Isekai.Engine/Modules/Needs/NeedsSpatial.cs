using Isekai.Engine.Core;
using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Provides minimal spatial checks for needs actions.
/// </summary>
internal static class NeedsSpatial
{
    public static bool IsWithinRange(Entity consumer, Entity resource, double maximumDistanceMeters)
    {
        if (!consumer.TryGetComponent<PositionComponent>(out var consumerPosition) ||
            consumerPosition is null ||
            !resource.TryGetComponent<PositionComponent>(out var resourcePosition) ||
            resourcePosition is null)
        {
            return true;
        }

        return GetDistanceMeters(consumerPosition.Position, resourcePosition.Position) <= Math.Max(0, maximumDistanceMeters);
    }

    public static bool IsWithinRange(Entity consumer, TerrainCellCoordinate coordinate, double maximumDistanceMeters)
    {
        if (!consumer.TryGetComponent<PositionComponent>(out var consumerPosition) || consumerPosition is null)
        {
            return true;
        }

        var target = new WorldPosition(coordinate.X, coordinate.Y, consumerPosition.Position.Z);
        return GetDistanceMeters(consumerPosition.Position, target) <= Math.Max(0, maximumDistanceMeters);
    }

    private static double GetDistanceMeters(WorldPosition first, WorldPosition second)
    {
        var dx = second.X - first.X;
        var dy = second.Y - first.Y;
        var dz = second.Z - first.Z;
        return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
    }
}
