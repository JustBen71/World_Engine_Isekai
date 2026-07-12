using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Modules.Body;
using Isekai.Engine.Modules.Materials;
using Isekai.Engine.Modules.Vitals;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for blood volume and vital state behavior.
/// </summary>
public sealed class VitalsModuleTests
{
    [Fact]
    public void VitalStateSystem_MarksEntityDeadWhenBloodIsTooLow()
    {
        var world = CreateWorld();
        var entity = world.CreateEntity();
        entity.AddComponent(new BloodComponent(CurrentVolumeLiters: 0.5, MaxVolumeLiters: 5));

        world.RegisterSystem(new VitalStateSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var vital = entity.GetComponent<VitalStateComponent>();

        Assert.False(vital.IsAlive);
        Assert.Equal("blood_loss", vital.DeathReason);
    }

    [Fact]
    public void VitalStateSystem_MarksEntityDeadWhenVitalPartIsDestroyed()
    {
        var world = CreateWorld();
        var entity = world.CreateEntity();
        entity.AddComponent(new BodyStateComponent(
            DefinitionReference<BodyDefinition>.From("body.test"),
            new[]
            {
                new BodyPartState("brain", Integrity: 0, MaxIntegrity: 10),
                new BodyPartState("arm", Integrity: 10, MaxIntegrity: 10)
            }));

        world.RegisterSystem(new VitalStateSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var vital = entity.GetComponent<VitalStateComponent>();

        Assert.False(vital.IsAlive);
        Assert.Equal("vital_part_destroyed:brain:brain", vital.DeathReason);
    }

    [Fact]
    public void VitalsModule_RegistersVitalStateSystem()
    {
        var world = CreateWorld();
        var module = new VitalsModule();

        module.RegisterSystems(world);

        Assert.Contains(world.Systems, system => system is VitalStateSystem);
    }

    private static WorldState CreateWorld()
    {
        var registry = new DefinitionRegistry();
        registry.Register(new MaterialDefinition(
            DefinitionId.From("material.test"),
            Density: 1000,
            SpecificHeatCapacity: 1000,
            ThermalConductivity: 1));
        registry.Register(new BodyDefinition(
            DefinitionId.From("body.test"),
            new[]
            {
                new BodyPartDefinition(
                    "brain",
                    "Brain",
                    DefinitionReference<MaterialDefinition>.From("material.test"),
                    MaxIntegrity: 10,
                    VitalRole: "brain"),
                new BodyPartDefinition(
                    "arm",
                    "Arm",
                    DefinitionReference<MaterialDefinition>.From("material.test"),
                    MaxIntegrity: 10)
            }));
        registry.Freeze();

        return new WorldState(
            new SimulationTime(),
            new EventBus(),
            registry,
            NoOpTraceLogger.Instance);
    }
}
