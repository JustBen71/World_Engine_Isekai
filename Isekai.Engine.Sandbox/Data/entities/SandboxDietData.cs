namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for a generic diet profile.
/// </summary>
public sealed record SandboxDietData(Dictionary<string, double> TagDigestibility);
