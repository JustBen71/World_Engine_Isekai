using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Core.Modules;
using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Modules.Materials;

/// <summary>
/// Provides the material module integration points.
/// </summary>
public sealed class MaterialModule : IEngineModule
{
    /// <inheritdoc />
    public string Name => "Materials";

    /// <inheritdoc />
    public void RegisterDefinitionTypes(DefinitionTypeRegistry registry)
    {
        registry.RegisterMaterialDefinitions();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IDefinitionValidator> CreateDefinitionValidators()
    {
        return new IDefinitionValidator[] { new MaterialDefinitionValidator() };
    }

    /// <inheritdoc />
    public void RegisterSystems(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        world.RegisterSystem(new MaterialMassSystem());
    }
}
