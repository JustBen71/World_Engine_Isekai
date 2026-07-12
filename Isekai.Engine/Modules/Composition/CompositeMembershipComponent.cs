using Isekai.Engine.Core;
using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Composition;

/// <summary>
/// Marks an entity as a part currently used by a composite entity.
/// </summary>
public sealed record CompositeMembershipComponent(
    EntityId CompositeEntityId,
    string Role,
    bool IsStructural) : IComponent;
