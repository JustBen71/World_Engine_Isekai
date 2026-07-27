namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Represents a direction or displacement vector in world meters.
/// </summary>
public readonly record struct WorldVector(double X, double Y, double Z)
{
    /// <summary>
    /// Gets the vector length.
    /// </summary>
    public double Length => Math.Sqrt((X * X) + (Y * Y) + (Z * Z));

    /// <summary>
    /// Returns true when all values are finite and the vector has a positive length.
    /// </summary>
    public bool IsValidDirection => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z) && Length > 0;

    /// <summary>
    /// Returns a normalized vector.
    /// </summary>
    public WorldVector Normalize()
    {
        var length = Length;
        return length <= 0 ? new WorldVector(0, 0, 0) : new WorldVector(X / length, Y / length, Z / length);
    }
}
