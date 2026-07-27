namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for normalized water quality.
/// </summary>
public sealed record SandboxWaterQualityData(
    double BacterialContamination,
    double ChemicalContamination,
    double Salinity);
