namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for an entity held by another sandbox entity.
/// </summary>
public sealed record SandboxHeldEntityData(
    string Entity,
    string Slot,
    double GripQuality);
