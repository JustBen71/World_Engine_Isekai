using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Stores an entity hydration reserve. Maximum means full, zero means depleted.
/// </summary>
public sealed record HydrationNeedComponent(
    double CurrentHydration,
    double MaximumHydration,
    double PassiveConsumptionPerSecond) : IComponent;
