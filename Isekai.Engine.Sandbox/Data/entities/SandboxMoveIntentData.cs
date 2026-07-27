namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for a local movement intent.
/// </summary>
public sealed record SandboxMoveIntentData(
    double DirectionX,
    double DirectionY,
    double DirectionZ,
    double DesiredDistanceMeters,
    string Mode);
