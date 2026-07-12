namespace Isekai.Engine.Modules.Injuries;

/// <summary>
/// Describes how strongly an injury currently bleeds.
/// </summary>
public enum BleedingSeverity
{
    /// <summary>
    /// The injury does not bleed.
    /// </summary>
    None,

    /// <summary>
    /// The injury bleeds lightly and can usually stabilize naturally.
    /// </summary>
    Minor,

    /// <summary>
    /// The injury bleeds enough to require attention.
    /// </summary>
    Moderate,

    /// <summary>
    /// The injury bleeds heavily and can become life-threatening.
    /// </summary>
    Severe,

    /// <summary>
    /// The injury causes extreme blood loss.
    /// </summary>
    Massive
}
