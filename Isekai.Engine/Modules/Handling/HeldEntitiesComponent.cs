using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Handling;

/// <summary>
/// Stores the entities currently held by an actor.
/// </summary>
public sealed record HeldEntitiesComponent : IComponent
{
    /// <summary>
    /// Creates a held entities component.
    /// </summary>
    public HeldEntitiesComponent(IReadOnlyCollection<HeldEntity> Entities)
    {
        this.Entities = Entities?.ToArray() ?? throw new ArgumentNullException(nameof(Entities));
    }

    /// <summary>
    /// Gets the held entities.
    /// </summary>
    public IReadOnlyCollection<HeldEntity> Entities { get; }
}
