using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Modules.Injuries;

/// <summary>
/// Stores one runtime injury applied to a body part.
/// </summary>
public sealed record InjuryState(
    DefinitionReference<InjuryDefinition> Injury,
    string BodyPartId,
    double Severity,
    BleedingSeverity? BleedingSeverity = null,
    bool IsTreated = false);
