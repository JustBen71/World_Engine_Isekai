using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Describes normalized contamination levels an entity can tolerate while drinking.
/// </summary>
public sealed record WaterToleranceComponent(
    double BacterialTolerance,
    double ChemicalTolerance,
    double SalinityTolerance) : IComponent;
