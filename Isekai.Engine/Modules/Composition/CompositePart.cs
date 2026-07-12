using Isekai.Engine.Core;

namespace Isekai.Engine.Modules.Composition;

/// <summary>
/// References one important physical entity used by a composite entity.
/// </summary>
public sealed record CompositePart(
    string Role,
    EntityId EntityId,
    bool IsStructural);
