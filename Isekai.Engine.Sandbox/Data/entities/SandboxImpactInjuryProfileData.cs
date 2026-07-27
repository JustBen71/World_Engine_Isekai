namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model configuring impact-to-injury conversion for one entity.
/// </summary>
public sealed record SandboxImpactInjuryProfileData(SandboxImpactInjuryRuleData[] Rules);
