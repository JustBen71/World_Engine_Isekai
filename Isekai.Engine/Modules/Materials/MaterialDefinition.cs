using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Modules.Materials;

/// <summary>
/// Defines universal physical material data used by simulation systems.
/// </summary>
public sealed record MaterialDefinition(
    DefinitionId Id,
    double Density,
    double SpecificHeatCapacity,
    double ThermalConductivity) : IDefinition;
