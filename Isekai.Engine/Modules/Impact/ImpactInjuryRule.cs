using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Modules.Injuries;

namespace Isekai.Engine.Modules.Impact;

/// <summary>
/// Describes how one impact outcome becomes one runtime injury on a target entity.
/// </summary>
public sealed record ImpactInjuryRule(
    ImpactOutcome Outcome,
    DefinitionReference<InjuryDefinition> Injury,
    double MinimumSeverity,
    double SeverityPerImpactRatio,
    BleedingSeverity? BleedingSeverity = null);
