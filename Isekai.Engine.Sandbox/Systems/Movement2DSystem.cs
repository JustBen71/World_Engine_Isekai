using Isekai.Engine.Core.System;
using Isekai.Engine.Sandbox.Components;

namespace Isekai.Engine.Sandbox.Systems;

/// <summary>
/// Moves sandbox entities on a bounded terminal grid.
/// </summary>
public sealed class Movement2DSystem : IWorldSystem
{
    private readonly int _width;
    private readonly int _height;

    /// <summary>
    /// Creates a movement system constrained by grid dimensions.
    /// </summary>
    public Movement2DSystem(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than zero.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than zero.");
        }

        _width = width;
        _height = height;
    }

    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entity in context.World.EntitiesWith<Position2DComponent, Velocity2DComponent>())
        {
            var position = entity.GetComponent<Position2DComponent>();
            var velocity = entity.GetComponent<Velocity2DComponent>();

            var nextX = Wrap(position.X + velocity.DeltaX, _width);
            var nextY = Wrap(position.Y + velocity.DeltaY, _height);

            entity.SetComponent(new Position2DComponent(nextX, nextY));
        }
    }

    private static int Wrap(int value, int max)
    {
        var wrapped = value % max;
        return wrapped < 0 ? wrapped + max : wrapped;
    }
}
