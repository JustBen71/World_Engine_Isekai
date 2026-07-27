using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Stores the last food consumption result resolved for an entity.
/// </summary>
public sealed record ConsumeActionResultComponent(ConsumeActionResult Result) : IComponent;
