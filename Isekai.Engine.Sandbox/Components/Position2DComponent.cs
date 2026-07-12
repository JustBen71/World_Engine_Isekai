using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Sandbox.Components;

/// <summary>
/// Stores a sandbox-only position on a terminal grid.
/// </summary>
public sealed record Position2DComponent(int X, int Y) : IComponent;
