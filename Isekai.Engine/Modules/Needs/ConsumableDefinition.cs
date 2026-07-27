using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Defines generic nutrition values for one unit of a consumable resource.
/// </summary>
public sealed record ConsumableDefinition(
    DefinitionId Id,
    string DisplayName,
    IReadOnlyCollection<string> Tags,
    double EnergyPerUnit,
    double HydrationPerUnit,
    double ToxicityPerUnit,
    double DefaultDigestibility) : IDefinition;
