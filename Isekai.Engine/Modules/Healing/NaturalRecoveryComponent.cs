using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Healing;

/// <summary>
/// Describes how well an entity naturally recovers from injuries.
/// </summary>
public sealed record NaturalRecoveryComponent(
    double TissueRecoveryPerSecond,
    double BleedingRecoveryPerSecond) : IComponent;
