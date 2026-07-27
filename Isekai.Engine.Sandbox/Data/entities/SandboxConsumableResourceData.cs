namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for an entity consumable stock.
/// </summary>
public sealed record SandboxConsumableResourceData(
    string Definition,
    double CurrentQuantity,
    double MaximumQuantity,
    double RegenerationPerSecond,
    bool RemoveEntityWhenEmpty);
