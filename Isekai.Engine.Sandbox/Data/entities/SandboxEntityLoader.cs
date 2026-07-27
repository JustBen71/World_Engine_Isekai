using System.Text.Json;
using System.Text.Json.Serialization;
using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Modules.Body;
using Isekai.Engine.Modules.BodyCapabilities;
using Isekai.Engine.Modules.Composition;
using Isekai.Engine.Modules.Handling;
using Isekai.Engine.Modules.Healing;
using Isekai.Engine.Modules.Injuries;
using Isekai.Engine.Modules.Impact;
using Isekai.Engine.Modules.Materials;
using Isekai.Engine.Modules.Needs;
using Isekai.Engine.Modules.Temperature;
using Isekai.Engine.Modules.Terrain;
using Isekai.Engine.Modules.Vitals;
using Isekai.Engine.Sandbox.Components;

namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// Loads sandbox-only entities from JSON into a world.
/// </summary>
public sealed class SandboxEntityLoader
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Creates a sandbox entity loader.
    /// </summary>
    public SandboxEntityLoader()
    {
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    /// <summary>
    /// Loads sandbox entities from a JSON file into a world.
    /// </summary>
    public void LoadFileInto(string filePath, WorldState world)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(world);

        var json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<SandboxEntityData>(json, _jsonOptions) ??
                   throw new InvalidOperationException("Sandbox entity data could not be read.");

        var entitiesByName = new Dictionary<string, Entity>(StringComparer.Ordinal);
        foreach (var definition in data.Entities)
        {
            var entity = CreateEntity(world, definition);
            if (!entitiesByName.TryAdd(definition.Name, entity))
            {
                throw new InvalidOperationException($"Sandbox entity name '{definition.Name}' is duplicated.");
            }
        }

        foreach (var definition in data.Entities)
        {
            if (definition.Composite is null)
            {
                AddImpactRequestIfNeeded(entitiesByName[definition.Name], definition, entitiesByName);
                AddHandlingComponentsIfNeeded(entitiesByName[definition.Name], definition, entitiesByName);
                AddBodyImpactIfNeeded(entitiesByName[definition.Name], definition, entitiesByName);
                AddNeedsIntentsIfNeeded(entitiesByName[definition.Name], definition, entitiesByName);
                continue;
            }

            AddCompositeComponent(entitiesByName[definition.Name], definition, entitiesByName);
            AddImpactRequestIfNeeded(entitiesByName[definition.Name], definition, entitiesByName);
            AddHandlingComponentsIfNeeded(entitiesByName[definition.Name], definition, entitiesByName);
            AddBodyImpactIfNeeded(entitiesByName[definition.Name], definition, entitiesByName);
            AddNeedsIntentsIfNeeded(entitiesByName[definition.Name], definition, entitiesByName);
        }
    }

    private static Entity CreateEntity(WorldState world, SandboxEntityDefinition definition)
    {
        var entity = world.CreateEntity();
        entity.AddComponent(new NameComponent(definition.Name));
        entity.AddComponent(new DisplayGlyphComponent(definition.Glyph));
        if (definition.Position is not null)
        {
            entity.AddComponent(new Position2DComponent(definition.Position.X, definition.Position.Y));
            entity.AddComponent(new PositionComponent(new WorldPosition(definition.Position.X, definition.Position.Y, 0)));
        }

        entity.AddComponent(new TemperatureComponent(definition.Temperature));

        var materials = ReadMaterials(definition);
        if (materials.Count > 0)
        {
            entity.AddComponent(new MaterialCompositionComponent(materials));
        }

        if (!string.IsNullOrWhiteSpace(definition.Body))
        {
            entity.AddComponent(new BodyComponent(DefinitionReference<BodyDefinition>.From(definition.Body)));
        }

        if (definition.Velocity is not null && definition.Position is not null)
        {
            entity.AddComponent(new Velocity2DComponent(definition.Velocity.DeltaX, definition.Velocity.DeltaY));
        }

        if (definition.ThermalSensitivity is not null)
        {
            entity.AddComponent(new ThermalSensitivityComponent(
                definition.ThermalSensitivity.ComfortableMinimum,
                definition.ThermalSensitivity.ComfortableMaximum));
        }

        if (definition.Blood is not null)
        {
            entity.AddComponent(new BloodComponent(
                definition.Blood.CurrentVolumeLiters,
                definition.Blood.MaxVolumeLiters));
            entity.AddComponent(new VitalStateComponent(IsAlive: true));
        }

        if (definition.NaturalRecovery is not null)
        {
            entity.AddComponent(new NaturalRecoveryComponent(
                definition.NaturalRecovery.TissueRecoveryPerSecond,
                definition.NaturalRecovery.BleedingRecoveryPerSecond));
        }

        if (definition.Injuries is { Length: > 0 })
        {
            entity.AddComponent(new InjuryComponent(definition.Injuries
                .Select(injury => new InjuryState(
                    DefinitionReference<InjuryDefinition>.From(injury.Injury),
                    injury.BodyPart,
                    injury.Severity,
                    injury.BleedingSeverity))
                .ToArray()));
        }

        if (definition.ContactSurface is not null)
        {
            entity.AddComponent(new ContactSurfaceComponent(
                definition.ContactSurface.Hardness,
                definition.ContactSurface.Sharpness,
                definition.ContactSurface.Penetration,
                definition.ContactSurface.EdgeRetention,
                definition.ContactSurface.ContactArea));
        }

        if (definition.ImpactResistance is not null)
        {
            entity.AddComponent(new ImpactResistanceComponent(
                definition.ImpactResistance.Hardness,
                definition.ImpactResistance.Toughness,
                definition.ImpactResistance.FractureResistance));
        }

        if (definition.ImpactInjuryProfile is not null)
        {
            entity.AddComponent(new ImpactInjuryProfileComponent(definition.ImpactInjuryProfile.Rules
                .Select(rule => new ImpactInjuryRule(
                    rule.Outcome,
                    DefinitionReference<InjuryDefinition>.From(rule.Injury),
                    rule.MinimumSeverity,
                    rule.SeverityPerImpactRatio,
                    rule.BleedingSeverity))
                .ToArray()));
        }

        if (definition.GripCapability is not null)
        {
            entity.AddComponent(new GripCapabilityComponent(
                definition.GripCapability.MaxGripForce,
                definition.GripCapability.ManipulationForce,
                definition.GripCapability.Precision));
        }

        if (definition.BodyImpactCapability is not null)
        {
            entity.AddComponent(new BodyImpactCapabilityComponent(
                definition.BodyImpactCapability.MaxForce,
                definition.BodyImpactCapability.ImpactForce,
                definition.BodyImpactCapability.Precision));
        }

        if (definition.BodyContactSurfaces is { Length: > 0 })
        {
            entity.AddComponent(new BodyContactSurfacesComponent(definition.BodyContactSurfaces
                .Select(surface => new BodyContactSurface(
                    surface.Role,
                    surface.BodyPart,
                    surface.Hardness,
                    surface.Sharpness,
                    surface.Penetration,
                    surface.EdgeRetention,
                    surface.ContactArea))
                .ToArray()));
        }

        if (definition.Needs is not null)
        {
            entity.AddComponent(new EnergyNeedComponent(
                definition.Needs.CurrentEnergy,
                definition.Needs.MaximumEnergy,
                definition.Needs.EnergyConsumptionPerSecond));
            entity.AddComponent(new HydrationNeedComponent(
                definition.Needs.CurrentHydration,
                definition.Needs.MaximumHydration,
                definition.Needs.HydrationConsumptionPerSecond));
        }

        if (definition.Diet is not null)
        {
            entity.AddComponent(new DietComponent(definition.Diet.TagDigestibility));
        }

        if (definition.WaterTolerance is not null)
        {
            entity.AddComponent(new WaterToleranceComponent(
                definition.WaterTolerance.BacterialTolerance,
                definition.WaterTolerance.ChemicalTolerance,
                definition.WaterTolerance.SalinityTolerance));
        }

        if (definition.Consumable is not null)
        {
            entity.AddComponent(new ConsumableResourceComponent(
                DefinitionReference<ConsumableDefinition>.From(definition.Consumable.Definition),
                definition.Consumable.CurrentQuantity,
                definition.Consumable.MaximumQuantity,
                definition.Consumable.RegenerationPerSecond,
                definition.Consumable.RemoveEntityWhenEmpty));
        }

        if (definition.WaterSource is not null)
        {
            entity.AddComponent(new WaterSourceComponent(
                definition.WaterSource.CurrentVolumeLiters,
                definition.WaterSource.MaximumVolumeLiters,
                new WaterQuality(
                    definition.WaterSource.Quality.BacterialContamination,
                    definition.WaterSource.Quality.ChemicalContamination,
                    definition.WaterSource.Quality.Salinity),
                definition.WaterSource.RefillLitersPerSecond));
        }

        return entity;
    }

    private static IReadOnlyCollection<MaterialQuantity> ReadMaterials(SandboxEntityDefinition definition)
    {
        var materials = new List<MaterialQuantity>();

        if (!string.IsNullOrWhiteSpace(definition.Material) && definition.Volume > 0)
        {
            materials.Add(new MaterialQuantity(
                DefinitionReference<MaterialDefinition>.From(definition.Material),
                definition.Volume));
        }

        if (definition.Materials is not null)
        {
            materials.AddRange(definition.Materials
                .Where(material => material.Volume > 0)
                .Select(material => new MaterialQuantity(
                    DefinitionReference<MaterialDefinition>.From(material.Material),
                    material.Volume)));
        }

        return materials;
    }

    private static void AddCompositeComponent(
        Entity entity,
        SandboxEntityDefinition definition,
        IReadOnlyDictionary<string, Entity> entitiesByName)
    {
        if (definition.Composite is null)
        {
            return;
        }

        var parts = definition.Composite.Parts
            .Select(part => CreateCompositePart(part, entitiesByName))
            .ToArray();

        entity.AddComponent(new CompositeComponent(parts));
    }

    private static CompositePart CreateCompositePart(
        SandboxCompositePartData part,
        IReadOnlyDictionary<string, Entity> entitiesByName)
    {
        if (!entitiesByName.TryGetValue(part.Entity, out var partEntity))
        {
            throw new InvalidOperationException($"Composite part '{part.Entity}' could not be resolved.");
        }

        return new CompositePart(part.Role, partEntity.Id, part.IsStructural);
    }

    private static void AddImpactRequestIfNeeded(
        Entity entity,
        SandboxEntityDefinition definition,
        IReadOnlyDictionary<string, Entity> entitiesByName)
    {
        if (definition.Impact is null)
        {
            return;
        }

        if (!entitiesByName.TryGetValue(definition.Impact.ContactEntity, out var contactEntity))
        {
            throw new InvalidOperationException($"Impact contact entity '{definition.Impact.ContactEntity}' could not be resolved.");
        }

        if (!entitiesByName.TryGetValue(definition.Impact.TargetEntity, out var targetEntity))
        {
            throw new InvalidOperationException($"Impact target entity '{definition.Impact.TargetEntity}' could not be resolved.");
        }

        entity.AddComponent(new ImpactRequestComponent(
            contactEntity.Id,
            targetEntity.Id,
            definition.Impact.Force,
            definition.Impact.TargetBodyPart));
    }

    private static void AddHandlingComponentsIfNeeded(
        Entity entity,
        SandboxEntityDefinition definition,
        IReadOnlyDictionary<string, Entity> entitiesByName)
    {
        if (definition.HeldEntities is { Length: > 0 })
        {
            entity.AddComponent(new HeldEntitiesComponent(definition.HeldEntities
                .Select(held => CreateHeldEntity(held, entitiesByName))
                .ToArray()));
        }

        if (definition.HandledImpact is not null)
        {
            entity.AddComponent(CreateHandledImpact(definition.HandledImpact, entitiesByName));
        }
    }

    private static HeldEntity CreateHeldEntity(
        SandboxHeldEntityData held,
        IReadOnlyDictionary<string, Entity> entitiesByName)
    {
        if (!entitiesByName.TryGetValue(held.Entity, out var heldEntity))
        {
            throw new InvalidOperationException($"Held entity '{held.Entity}' could not be resolved.");
        }

        return new HeldEntity(heldEntity.Id, held.Slot, held.GripQuality);
    }

    private static HandledImpactComponent CreateHandledImpact(
        SandboxHandledImpactData impact,
        IReadOnlyDictionary<string, Entity> entitiesByName)
    {
        if (!entitiesByName.TryGetValue(impact.HeldEntity, out var heldEntity))
        {
            throw new InvalidOperationException($"Handled impact held entity '{impact.HeldEntity}' could not be resolved.");
        }

        if (!entitiesByName.TryGetValue(impact.ContactEntity, out var contactEntity))
        {
            throw new InvalidOperationException($"Handled impact contact entity '{impact.ContactEntity}' could not be resolved.");
        }

        if (!entitiesByName.TryGetValue(impact.TargetEntity, out var targetEntity))
        {
            throw new InvalidOperationException($"Handled impact target entity '{impact.TargetEntity}' could not be resolved.");
        }

        return new HandledImpactComponent(
            heldEntity.Id,
            contactEntity.Id,
            targetEntity.Id,
            impact.Effort,
            impact.TicksRemaining,
            impact.TicksBetweenImpacts,
            TargetBodyPartId: impact.TargetBodyPart);
    }

    private static void AddBodyImpactIfNeeded(
        Entity entity,
        SandboxEntityDefinition definition,
        IReadOnlyDictionary<string, Entity> entitiesByName)
    {
        if (definition.BodyImpact is null)
        {
            return;
        }

        if (!entitiesByName.TryGetValue(definition.BodyImpact.TargetEntity, out var targetEntity))
        {
            throw new InvalidOperationException($"Body impact target entity '{definition.BodyImpact.TargetEntity}' could not be resolved.");
        }

        entity.AddComponent(new BodyImpactComponent(
            definition.BodyImpact.ContactRole,
            targetEntity.Id,
            definition.BodyImpact.Effort,
            definition.BodyImpact.TicksRemaining,
            definition.BodyImpact.TicksBetweenImpacts,
            TargetBodyPartId: definition.BodyImpact.TargetBodyPart));
    }

    private static void AddNeedsIntentsIfNeeded(
        Entity entity,
        SandboxEntityDefinition definition,
        IReadOnlyDictionary<string, Entity> entitiesByName)
    {
        if (definition.ConsumeIntent is not null)
        {
            if (!entitiesByName.TryGetValue(definition.ConsumeIntent.ResourceEntity, out var resourceEntity))
            {
                throw new InvalidOperationException($"Consume intent resource '{definition.ConsumeIntent.ResourceEntity}' could not be resolved.");
            }

            entity.AddComponent(new ConsumeIntentComponent(new ConsumeIntent(
                entity.Id,
                resourceEntity.Id,
                definition.ConsumeIntent.RequestedQuantity,
                definition.ConsumeIntent.MaximumDistanceMeters)));
        }

        if (definition.DrinkIntent is not null)
        {
            if (!entitiesByName.TryGetValue(definition.DrinkIntent.SourceEntity, out var sourceEntity))
            {
                throw new InvalidOperationException($"Drink intent source '{definition.DrinkIntent.SourceEntity}' could not be resolved.");
            }

            entity.AddComponent(new DrinkFromEntityIntentComponent(new DrinkFromEntityIntent(
                entity.Id,
                sourceEntity.Id,
                definition.DrinkIntent.RequestedVolumeLiters,
                definition.DrinkIntent.MaximumDistanceMeters)));
        }
    }
}
