using Isekai.Engine.Core;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Tests.TestDoubles;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for engine diagnostic traces.
/// </summary>
public sealed class DiagnosticsTests
{
    [Fact]
    public void FileTraceLogger_WritesNestedFunctionBlocksAtLevel10()
    {
        var logPath = CreateTempLogPath();

        try
        {
            using (var logger = new FileTraceLogger(logPath, EngineLogLevels.FullTrace))
            {
                var world = new WorldState(logger);
                var entity = world.CreateEntity();

                entity.AddComponent(new TestComponent(5));
                world.EntitiesWith<TestComponent>();
            }

            var log = File.ReadAllText(logPath);

            Assert.Contains("WorldState.CreateEntity => {", log);
            Assert.Contains("  Entity.ctor => {", log);
            Assert.Contains("} <= WorldState.CreateEntity", log);
            Assert.Contains("WorldState.EntitiesWith<TComponent> => {", log);
            Assert.Contains("  Entity.HasComponent => {", log);
            Assert.Contains("} <= WorldState.EntitiesWith<TComponent>", log);
        }
        finally
        {
            DeleteTempLog(logPath);
        }
    }

    [Fact]
    public void FileTraceLogger_DoesNotWriteFullTraceWhenLevelIsBelow10()
    {
        var logPath = CreateTempLogPath();

        try
        {
            using (var logger = new FileTraceLogger(logPath, EngineLogLevels.FullTrace - 1))
            {
                var world = new WorldState(logger);
                world.CreateEntity();
            }

            var log = File.ReadAllText(logPath);

            Assert.Equal(string.Empty, log);
        }
        finally
        {
            DeleteTempLog(logPath);
        }
    }

    private static string CreateTempLogPath()
    {
        return Path.Combine(Path.GetTempPath(), $"world-engine-{Guid.NewGuid():N}.log");
    }

    private static void DeleteTempLog(string logPath)
    {
        if (File.Exists(logPath))
        {
            File.Delete(logPath);
        }
    }
}
