using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Exceptions;

namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Validates terrain grid definitions before they are used by a generator.
/// </summary>
public sealed class TerrainGridDefinitionValidator : IDefinitionValidator
{
    /// <inheritdoc />
    public void Validate(DefinitionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var errors = registry.Definitions
            .OfType<TerrainGridDefinition>()
            .SelectMany(ValidateGrid)
            .ToArray();

        if (errors.Length > 0)
        {
            throw new InvalidDefinitionDataException(string.Join(Environment.NewLine, errors));
        }
    }

    private static IEnumerable<string> ValidateGrid(TerrainGridDefinition grid)
    {
        if (grid.WidthInCells <= 0)
        {
            yield return $"Terrain grid '{grid.Id}' must have a width greater than zero.";
        }

        if (grid.HeightInCells <= 0)
        {
            yield return $"Terrain grid '{grid.Id}' must have a height greater than zero.";
        }

        if (!double.IsFinite(grid.CellSizeMeters) || grid.CellSizeMeters <= 0)
        {
            yield return $"Terrain grid '{grid.Id}' must have a finite cell size greater than zero.";
        }

        if (!double.IsFinite(grid.MaxElevationMeters) || grid.MaxElevationMeters < 0)
        {
            yield return $"Terrain grid '{grid.Id}' must have a finite non-negative max elevation.";
        }
    }
}
