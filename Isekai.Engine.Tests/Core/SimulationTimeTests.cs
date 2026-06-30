using Isekai.Engine.Core;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for simulation time progression.
/// </summary>
public sealed class SimulationTimeTests
{
    [Fact]
    public void Advance_UpdatesElapsedDeltaAndTickCount()
    {
        var time = new SimulationTime();
        var delta = TimeSpan.FromSeconds(2);

        time.Advance(delta);

        Assert.Equal(delta, time.Elapsed);
        Assert.Equal(delta, time.Delta);
        Assert.Equal<ulong>(1, time.TickCount);
    }
}
