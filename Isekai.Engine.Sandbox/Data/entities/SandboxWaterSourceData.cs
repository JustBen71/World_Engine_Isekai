namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for an entity water source.
/// </summary>
public sealed record SandboxWaterSourceData(
    double CurrentVolumeLiters,
    double MaximumVolumeLiters,
    SandboxWaterQualityData Quality,
    double RefillLitersPerSecond);
