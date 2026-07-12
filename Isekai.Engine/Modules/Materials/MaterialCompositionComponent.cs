using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Materials;

/// <summary>
/// Stores the material composition of an entity.
/// </summary>
public sealed record MaterialCompositionComponent : IComponent
{
    /// <summary>
    /// Creates a material composition component.
    /// </summary>
    public MaterialCompositionComponent(IReadOnlyCollection<MaterialQuantity> materials)
    {
        Materials = materials?.ToArray() ?? throw new ArgumentNullException(nameof(materials));
    }

    /// <summary>
    /// Gets all material quantities present in the entity.
    /// </summary>
    public IReadOnlyCollection<MaterialQuantity> Materials { get; }
}
