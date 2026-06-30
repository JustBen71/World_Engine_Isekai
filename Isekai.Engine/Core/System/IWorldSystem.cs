namespace Isekai.Engine.Core.System;

/// <summary>
/// Represents simulation logic executed during a world tick.
/// </summary>
public interface IWorldSystem
{
    /// <summary>
    /// Executes this system for the current tick.
    /// </summary>
    void Execute(WorldSystemExecutionContext context);
}
