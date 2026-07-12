using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Modules.Injuries;

/// <summary>
/// Defines a generic injury type that can be applied to a body part.
/// </summary>
public sealed record InjuryDefinition(
    DefinitionId Id,
    string Name,
    double IntegrityLossPerSeverityPerSecond,
    BleedingSeverity DefaultBleedingSeverity) : IDefinition;
