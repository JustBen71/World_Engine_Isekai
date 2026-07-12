namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for one material quantity on a sandbox entity.
/// </summary>
public sealed record SandboxMaterialData(
    string Material,
    double Volume);
