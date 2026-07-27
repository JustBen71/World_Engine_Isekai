using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Registers needs module definition types with the generic data loader.
/// </summary>
public static class NeedsDefinitionTypeRegistration
{
    /// <summary>
    /// Registers all needs module definition types.
    /// </summary>
    public static void RegisterNeedsDefinitions(this DefinitionTypeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        registry.Register<ConsumableDefinition>("needs.consumable");
    }
}
