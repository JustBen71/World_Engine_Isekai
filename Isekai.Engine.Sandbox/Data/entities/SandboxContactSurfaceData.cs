namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for contact surface coefficients in the sandbox.
/// </summary>
public sealed record SandboxContactSurfaceData(
    double Hardness,
    double Sharpness,
    double Penetration,
    double EdgeRetention,
    double ContactArea);
