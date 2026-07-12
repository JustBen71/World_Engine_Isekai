using Isekai.Engine.Core;
using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Healing;

/// <summary>
/// Selects the target currently treated by a healing-capable entity.
/// </summary>
public sealed record HealingTargetComponent(EntityId TargetEntityId, string? BodyPartId = null) : IComponent;
