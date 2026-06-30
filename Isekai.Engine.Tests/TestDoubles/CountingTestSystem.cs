using Isekai.Engine.Core.System;

namespace Isekai.Engine.Tests.TestDoubles;

/// <summary>
/// Test system that records each execution.
/// </summary>
public sealed class CountingTestSystem : IWorldSystem
{
    /// <summary>
    /// Gets the number of times the system has executed.
    /// </summary>
    public int ExecutionCount { get; private set; }

    /// <summary>
    /// Gets the latest delta time received by the system.
    /// </summary>
    public TimeSpan LastDeltaTime { get; private set; }

    /// <inheritdoc />
    public void Execute(WorldSystemExecutionContext context)
    {
        ExecutionCount++;
        LastDeltaTime = context.DeltaTime;
    }
}
