using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Modules.Body;

/// <summary>
/// Defines a physical body structure made of named parts.
/// </summary>
public sealed record BodyDefinition(DefinitionId Id, BodyPartDefinition[] Parts) : IDefinition;
