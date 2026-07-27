namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Provides shared numeric helpers for needs systems.
/// </summary>
internal static class NeedMath
{
    public static double ResolveDeltaSeconds(TimeSpan deltaTime)
    {
        var seconds = deltaTime.TotalSeconds;
        return double.IsFinite(seconds) ? Math.Max(0, seconds) : 0;
    }

    public static double ClampReserve(double value, double maximum)
    {
        if (!double.IsFinite(maximum) || maximum <= 0)
        {
            return 0;
        }

        if (!double.IsFinite(value))
        {
            return 0;
        }

        return Math.Clamp(value, 0, maximum);
    }

    public static double SafeNonNegative(double value)
    {
        return double.IsFinite(value) ? Math.Max(0, value) : 0;
    }

    public static bool IsPositiveFinite(double value)
    {
        return double.IsFinite(value) && value > 0;
    }
}
