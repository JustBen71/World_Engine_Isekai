using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Stores the continuous world position of an entity.
/// </summary>
public sealed record PositionComponent(WorldPosition Position) : IComponent;
