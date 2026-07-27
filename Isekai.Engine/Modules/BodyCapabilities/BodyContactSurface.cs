namespace Isekai.Engine.Modules.BodyCapabilities;

/// <summary>
/// Describes one body part that an entity is allowed to use as a contact surface.
/// </summary>
public sealed record BodyContactSurface(
    string Role,
    string BodyPartId,
    double Hardness,
    double Sharpness,
    double Penetration,
    double EdgeRetention,
    double ContactArea);
