namespace Isekai.Engine.Utilities;

/// <summary>
/// Provides small validation helpers shared by engine code.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Throws when the provided value is null.
    /// </summary>
    public static T NotNull<T>(T? value, string parameterName)
        where T : class
    {
        return value ?? throw new ArgumentNullException(parameterName);
    }
}
