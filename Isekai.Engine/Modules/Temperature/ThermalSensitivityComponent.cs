using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Temperature;

/// <summary>
/// Marks an entity as able to feel thermal comfort or stress.
/// </summary>
public sealed record ThermalSensitivityComponent(
    double ComfortableMinimumCelsius,
    double ComfortableMaximumCelsius) : IComponent;
