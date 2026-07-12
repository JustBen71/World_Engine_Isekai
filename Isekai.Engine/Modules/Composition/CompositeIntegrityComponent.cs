using Isekai.Engine.Core;
using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Composition;

/// <summary>
/// Stores the calculated structural state of a composite entity.
/// </summary>
public sealed record CompositeIntegrityComponent(
    double NormalizedIntegrity,
    bool IsStructurallyComplete,
    string[] MissingStructuralRoles,
    EntityId? WeakestStructuralPartId,
    string? WeakestStructuralRole) : IComponent;
