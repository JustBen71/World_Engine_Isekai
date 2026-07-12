using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Exceptions;

namespace Isekai.Engine.Modules.Materials;

/// <summary>
/// Validates universal material definitions before simulation starts.
/// </summary>
public sealed class MaterialDefinitionValidator : IDefinitionValidator
{
    /// <summary>
    /// Validates all material definitions in a registry.
    /// </summary>
    public void Validate(DefinitionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var errors = registry.Definitions
            .OfType<MaterialDefinition>()
            .SelectMany(ValidateMaterial)
            .ToArray();

        if (errors.Length > 0)
        {
            throw new InvalidDefinitionDataException(string.Join(Environment.NewLine, errors));
        }
    }

    private static IEnumerable<string> ValidateMaterial(MaterialDefinition material)
    {
        if (material.Density <= 0)
        {
            yield return $"Material '{material.Id}' must have a density greater than zero.";
        }

        if (material.SpecificHeatCapacity <= 0)
        {
            yield return $"Material '{material.Id}' must have a specific heat capacity greater than zero.";
        }

        if (material.ThermalConductivity < 0)
        {
            yield return $"Material '{material.Id}' cannot have negative thermal conductivity.";
        }
    }
}
