using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Healing;

/// <summary>
/// Marks an entity as able to treat injuries on another entity.
/// </summary>
public sealed record HealingCapabilityComponent(
    double TissueTreatmentPerSecond,
    double BleedingTreatmentPerSecond) : IComponent;
