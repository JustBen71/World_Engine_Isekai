using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Stores an entity energy reserve. Maximum means full, zero means depleted.
/// </summary>
public sealed record EnergyNeedComponent(
    double CurrentEnergy,
    double MaximumEnergy,
    double PassiveConsumptionPerSecond) : IComponent;
