using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Perception;

/// <summary>
/// Stores data-driven tags that describe an entity to perception.
/// </summary>
public sealed record PerceptionSignatureComponent(IReadOnlyCollection<string> Tags) : IComponent;
