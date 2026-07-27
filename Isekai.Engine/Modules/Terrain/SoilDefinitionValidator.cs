using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Exceptions;

namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Validates soil definitions before simulation starts.
/// </summary>
public sealed class SoilDefinitionValidator : IDefinitionValidator
{
    /// <inheritdoc />
    public void Validate(DefinitionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var errors = registry.Definitions
            .OfType<SoilDefinition>()
            .SelectMany(ValidateSoil)
            .ToArray();

        if (errors.Length > 0)
        {
            throw new InvalidDefinitionDataException(string.Join(Environment.NewLine, errors));
        }
    }

    private static IEnumerable<string> ValidateSoil(SoilDefinition soil)
    {
        if (string.IsNullOrWhiteSpace(soil.Id.Value))
        {
            yield return "Soil definition id cannot be empty.";
        }

        if (string.IsNullOrWhiteSpace(soil.DisplayName))
        {
            yield return $"Soil '{soil.Id}' must have a display name.";
        }

        if (!double.IsFinite(soil.MovementCostMultiplier) || soil.MovementCostMultiplier <= 0)
        {
            yield return $"Soil '{soil.Id}' must have a finite movement cost multiplier greater than zero.";
        }

        if (!IsRatio(soil.WaterRetention))
        {
            yield return $"Soil '{soil.Id}' must have water retention between zero and one.";
        }

        if (!IsRatio(soil.Drainage))
        {
            yield return $"Soil '{soil.Id}' must have drainage between zero and one.";
        }

        if (!IsRatio(soil.Fertility))
        {
            yield return $"Soil '{soil.Id}' must have fertility between zero and one.";
        }

        if (!double.IsFinite(soil.Hardness) || soil.Hardness < 0)
        {
            yield return $"Soil '{soil.Id}' must have finite non-negative hardness.";
        }
    }

    private static bool IsRatio(double value)
    {
        return double.IsFinite(value) && value >= 0 && value <= 1;
    }
}
