using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Temporary component carrying a movement intent for resolution by the engine.
/// </summary>
public sealed record MoveIntentComponent(MoveIntent Intent) : IComponent;
