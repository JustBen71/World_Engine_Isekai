using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.BodyCapabilities;

/// <summary>
/// Stores the body contact surfaces an entity is allowed to use.
/// </summary>
public sealed record BodyContactSurfacesComponent : IComponent
{
    /// <summary>
    /// Creates a body contact surface component.
    /// </summary>
    public BodyContactSurfacesComponent(IReadOnlyCollection<BodyContactSurface> surfaces)
    {
        Surfaces = surfaces?.ToArray() ?? throw new ArgumentNullException(nameof(surfaces));
    }

    /// <summary>
    /// Gets the authorized body contact surfaces.
    /// </summary>
    public IReadOnlyCollection<BodyContactSurface> Surfaces { get; }
}
