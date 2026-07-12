using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Vitals;

/// <summary>
/// Stores whether an entity is still biologically alive and why it died if it did.
/// </summary>
public sealed record VitalStateComponent(bool IsAlive, string? DeathReason = null) : IComponent;
