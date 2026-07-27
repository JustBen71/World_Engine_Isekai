namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for simplified perception capability.
/// </summary>
public sealed record SandboxPerceptionCapabilityData(
    double MaximumRangeMeters,
    double FieldOfViewDegrees,
    int MaximumPerceivedEntities);
