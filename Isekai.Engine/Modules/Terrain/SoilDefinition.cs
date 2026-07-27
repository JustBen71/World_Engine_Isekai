using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Defines physical soil properties used by terrain and future simulation modules.
/// </summary>
public sealed record SoilDefinition(
    DefinitionId Id,
    string DisplayName,
    double MovementCostMultiplier,
    double WaterRetention,
    double Drainage,
    double Fertility,
    double Hardness) : IDefinition;
