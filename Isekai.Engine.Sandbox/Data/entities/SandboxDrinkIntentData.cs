namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for a water drinking intent.
/// </summary>
public sealed record SandboxDrinkIntentData(
    string SourceEntity,
    double RequestedVolumeLiters,
    double MaximumDistanceMeters);
