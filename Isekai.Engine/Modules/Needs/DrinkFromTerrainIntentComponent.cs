using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Temporary component carrying a terrain water drinking intent.
/// </summary>
public sealed record DrinkFromTerrainIntentComponent(DrinkFromTerrainIntent Intent) : IComponent;
