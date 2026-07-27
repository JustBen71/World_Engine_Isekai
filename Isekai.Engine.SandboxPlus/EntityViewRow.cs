namespace Isekai.Engine.SandboxPlus;

using Isekai.Engine.Core;

/// <summary>
/// Immutable row displayed by the Sandbox++ entity table.
/// </summary>
internal sealed record EntityViewRow(
    EntityId EntityId,
    string Name,
    string Position,
    string Needs,
    string Temperature,
    string State,
    string Action);
