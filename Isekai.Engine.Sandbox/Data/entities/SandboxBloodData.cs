namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for initial blood state in the sandbox.
/// </summary>
public sealed record SandboxBloodData(
    double CurrentVolumeLiters,
    double MaxVolumeLiters);
