using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Modules.Healing;
using Isekai.Engine.Modules.Injuries;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for natural recovery and active treatment behavior.
/// </summary>
public sealed class HealingModuleTests
{
    [Fact]
    public void TreatmentSystem_ReducesTargetInjuryAndMarksItTreated()
    {
        var world = CreateWorld();
        var patient = world.CreateEntity();
        patient.AddComponent(new InjuryComponent(new[]
        {
            new InjuryState(
                DefinitionReference<InjuryDefinition>.From("injury.test"),
                "left_arm",
                Severity: 1,
                BleedingSeverity.Moderate)
        }));

        var healer = world.CreateEntity();
        healer.AddComponent(new HealingCapabilityComponent(
            TissueTreatmentPerSecond: 0.2,
            BleedingTreatmentPerSecond: 0.3));
        healer.AddComponent(new HealingTargetComponent(patient.Id));

        world.RegisterSystem(new TreatmentSystem());
        world.Tick(TimeSpan.FromSeconds(1));

        var injury = patient.GetComponent<InjuryComponent>().Injuries.Single();

        Assert.Equal(0.5, injury.Severity, precision: 3);
        Assert.True(injury.IsTreated);
    }

    [Fact]
    public void HealingModule_RegistersRecoveryAndTreatmentSystems()
    {
        var world = CreateWorld();
        var module = new HealingModule();

        module.RegisterSystems(world);

        Assert.Contains(world.Systems, system => system is NaturalRecoverySystem);
        Assert.Contains(world.Systems, system => system is TreatmentSystem);
    }

    private static WorldState CreateWorld()
    {
        var registry = new DefinitionRegistry();
        registry.Register(new InjuryDefinition(
            DefinitionId.From("injury.test"),
            Name: "Test injury",
            IntegrityLossPerSeverityPerSecond: 0,
            DefaultBleedingSeverity: BleedingSeverity.Moderate));
        registry.Freeze();

        return new WorldState(
            new SimulationTime(),
            new EventBus(),
            registry,
            NoOpTraceLogger.Instance);
    }
}
