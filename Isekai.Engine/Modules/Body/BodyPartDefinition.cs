using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Modules.Materials;

namespace Isekai.Engine.Modules.Body;

/// <summary>
/// Defines one universal body part in a body definition.
/// </summary>
public sealed record BodyPartDefinition(
    string Id,
    string Name,
    DefinitionReference<MaterialDefinition> Material,
    double MaxIntegrity,
    string? VitalRole = null);
