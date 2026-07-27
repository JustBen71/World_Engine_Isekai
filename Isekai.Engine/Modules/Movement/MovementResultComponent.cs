using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Stores the latest movement result for an entity.
/// </summary>
public sealed record MovementResultComponent(MovementResult Result) : IComponent;
