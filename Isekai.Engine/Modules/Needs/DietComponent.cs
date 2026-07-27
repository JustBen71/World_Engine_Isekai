using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Describes which consumable tags an entity can digest and how efficiently.
/// </summary>
public sealed record DietComponent(IReadOnlyDictionary<string, double> TagDigestibility) : IComponent;
