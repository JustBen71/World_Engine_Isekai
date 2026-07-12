using System.Text.Json;
using System.Text.Json.Serialization;
using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Modules.Body;
using Isekai.Engine.Modules.Composition;
using Isekai.Engine.Modules.Healing;
using Isekai.Engine.Modules.Injuries;
using Isekai.Engine.Modules.Impact;
using Isekai.Engine.Modules.Materials;
using Isekai.Engine.Modules.Temperature;
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
                continue;
            }

            AddCompositeComponent(entitiesByName[definition.Name], definition, entitiesByName);
            AddImpactRequestIfNeeded(entitiesByName[definition.Name], definition, entitiesByName);
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
}
