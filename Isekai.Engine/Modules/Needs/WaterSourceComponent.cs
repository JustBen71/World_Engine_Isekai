using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Stores a finite drinkable water stock carried by an entity.
/// </summary>
public sealed record WaterSourceComponent(
    double CurrentVolumeLiters,
    double MaximumVolumeLiters,
    WaterQuality Quality,
    double RefillLitersPerSecond = 0) : IComponent;
