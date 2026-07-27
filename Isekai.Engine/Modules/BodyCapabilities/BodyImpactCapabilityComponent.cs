using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.BodyCapabilities;

/// <summary>
/// Describes an entity's ability to produce impacts with its own body.
/// </summary>
public sealed record BodyImpactCapabilityComponent(
    double MaxForce,
    double ImpactForce,
    double Precision) : IComponent;
