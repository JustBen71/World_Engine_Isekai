namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for a food consumption intent.
/// </summary>
public sealed record SandboxConsumeIntentData(
    string ResourceEntity,
    double RequestedQuantity,
    double MaximumDistanceMeters);
