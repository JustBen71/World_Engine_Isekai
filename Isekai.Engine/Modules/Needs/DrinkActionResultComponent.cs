using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Stores the last drinking result resolved for an entity.
/// </summary>
public sealed record DrinkActionResultComponent(DrinkActionResult Result) : IComponent;
