namespace Isekai.Engine.Core;

/// <summary>
/// Describes the result of a completed simulation tick.
/// </summary>
public sealed record TickResult(TimeSpan DeltaTime, TimeSpan ElapsedTime, ulong TickCount, int ExecutedSystemCount);
