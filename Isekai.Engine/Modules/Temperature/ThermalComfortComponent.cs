using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Temperature;

/// <summary>
/// Stores calculated thermal comfort for a sensitive entity.
/// </summary>
public sealed record ThermalComfortComponent(
    double Comfort,
    double ColdStress,
    double HeatStress) : IComponent;
