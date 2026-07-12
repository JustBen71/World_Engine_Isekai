using Isekai.Engine.Modules.Injuries;

namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for one initial sandbox injury.
/// </summary>
public sealed record SandboxInjuryData(
    string Injury,
    string BodyPart,
    double Severity,
    BleedingSeverity? BleedingSeverity);
