namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model describing one authorized body contact surface.
/// </summary>
public sealed record SandboxBodyContactSurfaceData(
    string Role,
    string BodyPart,
    double Hardness,
    double Sharpness,
    double Penetration,
    double EdgeRetention,
    double ContactArea);
