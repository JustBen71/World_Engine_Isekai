using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Impact;

/// <summary>
/// Describes the active contact properties of an entity shape.
/// Values use 1.0 as the standard baseline; higher values perform better for that property.
/// </summary>
public sealed record ContactSurfaceComponent(
    double Hardness,
    double Sharpness,
    double Penetration,
    double EdgeRetention,
    double ContactArea) : IComponent;
