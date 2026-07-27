using Isekai.Engine.Core;
using Isekai.Engine.Interfaces;

namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Provides a generic mobility multiplier for an entity.
/// </summary>
public interface IMobilityProvider
{
    /// <summary>
    /// Gets a normalized mobility multiplier.
    /// </summary>
    double GetMobilityMultiplier(IWorldState world, Entity entity);
}
