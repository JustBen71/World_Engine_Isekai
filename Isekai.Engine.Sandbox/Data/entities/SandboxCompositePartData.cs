namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for one named entity used as an important composite part.
/// </summary>
public sealed record SandboxCompositePartData(
    string Role,
    string Entity,
    bool IsStructural);
