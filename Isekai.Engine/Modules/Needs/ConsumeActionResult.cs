using Isekai.Engine.Core;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Reports the resolution of one food consumption intent.
/// </summary>
public sealed record ConsumeActionResult(
    EntityId ConsumerEntityId,
    EntityId ResourceEntityId,
    double RequestedQuantity,
    double ConsumedQuantity,
    double EnergyReceived,
    double HydrationReceived,
    double ToxicityExposure,
    ConsumeOutcome Outcome);
