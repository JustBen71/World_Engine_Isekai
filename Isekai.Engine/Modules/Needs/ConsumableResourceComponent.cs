using Isekai.Engine.Core.Component;
using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Stores a finite consumable stock carried by an entity.
/// </summary>
public sealed record ConsumableResourceComponent(
    DefinitionReference<ConsumableDefinition> Definition,
    double CurrentQuantity,
    double MaximumQuantity,
    double RegenerationPerSecond = 0,
    bool RemoveEntityWhenEmpty = false) : IComponent;
