namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Stores normalized water contamination values between zero and one.
/// </summary>
public sealed class WaterQuality
{
    /// <summary>
    /// Represents clean fresh water.
    /// </summary>
    public static WaterQuality Clean { get; } = new(0, 0, 0);

    /// <summary>
    /// Creates a normalized water quality value.
    /// </summary>
    public WaterQuality(double bacterialContamination, double chemicalContamination, double salinity)
    {
        BacterialContamination = ValidateRatio(bacterialContamination, nameof(bacterialContamination));
        ChemicalContamination = ValidateRatio(chemicalContamination, nameof(chemicalContamination));
        Salinity = ValidateRatio(salinity, nameof(salinity));
    }

    /// <summary>
    /// Gets normalized bacterial contamination.
    /// </summary>
    public double BacterialContamination { get; }

    /// <summary>
    /// Gets normalized chemical contamination.
    /// </summary>
    public double ChemicalContamination { get; }

    /// <summary>
    /// Gets normalized salinity.
    /// </summary>
    public double Salinity { get; }

    private static double ValidateRatio(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0 || value > 1)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Water quality values must be finite and between zero and one.");
        }

        return value;
    }
}
