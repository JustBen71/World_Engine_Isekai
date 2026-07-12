namespace Isekai.Engine.Sandbox.World;

/// <summary>
/// Provides a deterministic sandbox map used only by the console validator.
/// </summary>
public sealed class SandboxMap
{
    private readonly TerrainKind[,] _terrain;

    /// <summary>
    /// Creates a deterministic sandbox map.
    /// </summary>
    public SandboxMap(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than zero.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than zero.");
        }

        Width = width;
        Height = height;
        _terrain = new TerrainKind[height, width];
        GenerateTerrain();
    }

    /// <summary>
    /// Gets the map width.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the map height.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the terrain at a map coordinate.
    /// </summary>
    public TerrainKind GetTerrain(int x, int y)
    {
        return _terrain[y, x];
    }

    /// <summary>
    /// Gets the simulated temperature at a map coordinate for the current tick.
    /// </summary>
    public double GetTemperature(int x, int y, ulong tick)
    {
        var baseTemperature = GetTerrain(x, y) switch
        {
            TerrainKind.Prairie => 22.0,
            TerrainKind.Forest => 19.5,
            TerrainKind.Water => 16.0,
            TerrainKind.Mountain => 12.0,
            _ => 20.0
        };

        var dayCycle = Math.Sin((tick + x * 0.7 + y * 0.35) / 6.0) * 2.5;
        return baseTemperature + dayCycle;
    }

    /// <summary>
    /// Calculates temperature statistics for the whole map.
    /// </summary>
    public MapTemperatureStats GetTemperatureStats(ulong tick)
    {
        var min = double.MaxValue;
        var max = double.MinValue;
        var sum = 0.0;

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var temperature = GetTemperature(x, y, tick);
                min = Math.Min(min, temperature);
                max = Math.Max(max, temperature);
                sum += temperature;
            }
        }

        return new MapTemperatureStats(sum / (Width * Height), min, max);
    }

    private void GenerateTerrain()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                _terrain[y, x] = ResolveTerrain(x, y);
            }
        }
    }

    private TerrainKind ResolveTerrain(int x, int y)
    {
        if (y >= Height - 2 || (x > Width - 7 && y > Height / 2))
        {
            return TerrainKind.Water;
        }

        if ((x >= 2 && x <= 6 && y >= 1 && y <= 4) ||
            (x >= Width - 8 && x <= Width - 4 && y >= 1 && y <= 3))
        {
            return TerrainKind.Forest;
        }

        if ((x >= Width / 2 - 2 && x <= Width / 2 + 2 && y >= Height / 2 - 1 && y <= Height / 2 + 1) ||
            (x == Width / 2 + 4 && y == Height / 2))
        {
            return TerrainKind.Mountain;
        }

        return TerrainKind.Prairie;
    }
}
