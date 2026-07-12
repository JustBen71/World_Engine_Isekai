using Isekai.Engine.Core.Component;
using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Modules.Body;

/// <summary>
/// Assigns a body definition to an entity.
/// </summary>
public sealed record BodyComponent(DefinitionReference<BodyDefinition> Body) : IComponent;
