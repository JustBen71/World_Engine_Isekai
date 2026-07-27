using Isekai.Engine.Modules.Impact;
using Isekai.Engine.Modules.Injuries;

namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model describing one impact-to-injury rule.
/// </summary>
public sealed record SandboxImpactInjuryRuleData(
    ImpactOutcome Outcome,
    string Injury,
    double MinimumSeverity,
    double SeverityPerImpactRatio,
    BleedingSeverity? BleedingSeverity);
