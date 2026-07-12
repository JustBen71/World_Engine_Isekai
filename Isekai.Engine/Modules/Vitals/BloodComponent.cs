using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Vitals;

/// <summary>
/// Stores the blood volume of a living entity.
/// </summary>
public sealed record BloodComponent : IComponent
{
    /// <summary>
    /// Creates a blood component.
    /// </summary>
    public BloodComponent(double CurrentVolumeLiters, double MaxVolumeLiters)
    {
        if (MaxVolumeLiters <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxVolumeLiters), MaxVolumeLiters, "Maximum blood volume must be greater than zero.");
        }

        this.MaxVolumeLiters = MaxVolumeLiters;
        this.CurrentVolumeLiters = Math.Clamp(CurrentVolumeLiters, 0, MaxVolumeLiters);
    }

    /// <summary>
    /// Gets the current blood volume in liters.
    /// </summary>
    public double CurrentVolumeLiters { get; }

    /// <summary>
    /// Gets the maximum blood volume in liters.
    /// </summary>
    public double MaxVolumeLiters { get; }

    /// <summary>
    /// Gets the remaining blood ratio from 0 to 1.
    /// </summary>
    public double Ratio => CurrentVolumeLiters / MaxVolumeLiters;
}
