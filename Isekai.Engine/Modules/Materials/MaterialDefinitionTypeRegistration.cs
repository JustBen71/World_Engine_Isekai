using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Modules.Materials;

/// <summary>
/// Registers material definition types with the generic data loader.
/// </summary>
public static class MaterialDefinitionTypeRegistration
{
    /// <summary>
    /// Registers all material module definition types.
    /// </summary>
    public static void RegisterMaterialDefinitions(this DefinitionTypeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        registry.Register<MaterialDefinition>("material.definition");
    }
}
