using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Sandbox.Components;

/// <summary>
/// Stores a sandbox-only grid velocity applied once per tick.
/// </summary>
public sealed record Velocity2DComponent(int DeltaX, int DeltaY) : IComponent;
