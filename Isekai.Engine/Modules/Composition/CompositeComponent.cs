using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Composition;

/// <summary>
/// Marks an entity as a composite made from important physical parts.
/// Integrated materials stay represented by the regular material composition component.
/// </summary>
public sealed record CompositeComponent : IComponent
{
    /// <summary>
    /// Creates a composite component.
    /// </summary>
    public CompositeComponent(IReadOnlyCollection<CompositePart> Parts)
    {
        this.Parts = Parts?.ToArray() ?? throw new ArgumentNullException(nameof(Parts));
    }

    /// <summary>
    /// Gets the important physical parts of the composite.
    /// </summary>
    public IReadOnlyCollection<CompositePart> Parts { get; }
}
