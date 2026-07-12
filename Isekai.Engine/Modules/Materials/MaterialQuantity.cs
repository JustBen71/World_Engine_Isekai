using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Modules.Materials;

/// <summary>
/// Describes how much volume of a material is present in an entity.
/// </summary>
public sealed record MaterialQuantity(
    DefinitionReference<MaterialDefinition> Material,
    double Volume);
