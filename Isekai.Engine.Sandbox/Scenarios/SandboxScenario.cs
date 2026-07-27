namespace Isekai.Engine.Sandbox.Scenarios;

/// <summary>
/// Describes one runnable sandbox scenario.
/// </summary>
public sealed record SandboxScenario(
    string Id,
    string Name,
    string Description,
    string EntityFileName,
    int DefaultTicks,
    bool UseLocalAmbientTemperatureProvider = false,
    bool ShowTerrainDiagnostics = false);
