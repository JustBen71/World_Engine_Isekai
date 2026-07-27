using Isekai.Engine.Core.System;
using Isekai.Engine.Modules.Terrain;
using Isekai.Engine.Sandbox.Components;

namespace Isekai.Engine.Sandbox.Systems;

/// <summary>
/// Mirrors engine world positions into the legacy sandbox grid position used by the terminal renderer.
/// </summary>
public sealed class SandboxPosition2DSyncSystem : IWorldSystem
{
    private readonly int _width;
    private readonly int _height;

    /// <summary>
    /// Creates the synchronization system.
    /// </summary>
    public SandboxPosition2DSyncSystem(int width, int height)
    {
        _width = width;
        _height = height;
    }

    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        foreach (var entity in context.World.EntitiesWith<PositionComponent>())
        {
            var position = entity.GetComponent<PositionComponent>().Position;
            var x = Math.Clamp((int)Math.Round(position.X), 0, _width - 1);
            var y = Math.Clamp((int)Math.Round(position.Y), 0, _height - 1);

            entity.SetComponent(new Position2DComponent(x, y));
        }
    }
}
