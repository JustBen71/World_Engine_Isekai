using Isekai.Engine.Core;

namespace Isekai.Engine.Modules.Handling;

/// <summary>
/// Describes one entity currently held by another entity.
/// </summary>
public sealed record HeldEntity(
    EntityId EntityId,
    string Slot,
    double GripQuality);
