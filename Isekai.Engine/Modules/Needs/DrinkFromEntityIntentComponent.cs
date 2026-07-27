using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Temporary component carrying an entity water drinking intent.
/// </summary>
public sealed record DrinkFromEntityIntentComponent(DrinkFromEntityIntent Intent) : IComponent;
