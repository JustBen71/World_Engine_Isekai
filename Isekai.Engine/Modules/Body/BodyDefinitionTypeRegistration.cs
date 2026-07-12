using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Modules.Body;

/// <summary>
/// Registers body definition types with the generic data loader.
/// </summary>
public static class BodyDefinitionTypeRegistration
{
    /// <summary>
    /// Registers all body module definition types.
    /// </summary>
    public static void RegisterBodyDefinitions(this DefinitionTypeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        registry.Register<BodyDefinition>("body.definition");
    }
}
