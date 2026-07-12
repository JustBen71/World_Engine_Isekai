using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Sandbox.Components;

/// <summary>
/// Stores a sandbox display name for an entity.
/// </summary>
public sealed record NameComponent(string Name) : IComponent;
