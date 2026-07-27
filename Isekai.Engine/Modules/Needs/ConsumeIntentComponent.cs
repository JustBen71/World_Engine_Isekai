using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Temporary component carrying a food consumption intent for resolution by systems.
/// </summary>
public sealed record ConsumeIntentComponent(ConsumeIntent Intent) : IComponent;
