using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Data.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Exceptions;
using Isekai.Engine.Tests.TestDoubles;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for loading neutral definitions from JSON data.
/// </summary>
public sealed class JsonDefinitionLoaderTests
{
    [Fact]
    public void DefinitionTypeRegistry_RegistersDefinitionTypeName()
    {
        var types = new DefinitionTypeRegistry();

        types.Register<TestDefinition>("test.definition");

        Assert.Contains("test.definition", types.TypeNames);
    }

    [Fact]
    public void DefinitionTypeRegistry_ThrowsWhenTypeNameAlreadyExists()
    {
        var types = new DefinitionTypeRegistry();
        types.Register<TestDefinition>("test.definition");

        Assert.Throws<DuplicateDefinitionTypeException>(() =>
            types.Register<AlternativeTestDefinition>("test.definition"));
    }

    [Fact]
    public void LoadJson_LoadsSingleDefinitionObject()
    {
        var loader = CreateLoader();
        const string json = """
        {
          "type": "test.definition",
          "id": "definition.one",
          "label": "neutral"
        }
        """;

        var registry = loader.LoadJson(json);

        var definition = registry.Get<TestDefinition>(DefinitionId.From("definition.one"));
        Assert.Equal("neutral", definition.Label);
    }

    [Fact]
    public void LoadJson_LoadsDefinitionArray()
    {
        var loader = CreateLoader();
        const string json = """
        [
          {
            "type": "test.definition",
            "id": "definition.one",
            "label": "first"
          },
          {
            "type": "test.definition",
            "id": "definition.two",
            "label": "second"
          }
        ]
        """;

        var registry = loader.LoadJson(json);

        Assert.Equal(2, registry.Definitions.Count);
        Assert.Equal("first", registry.Get<TestDefinition>(DefinitionId.From("definition.one")).Label);
        Assert.Equal("second", registry.Get<TestDefinition>(DefinitionId.From("definition.two")).Label);
    }

    [Fact]
    public void LoadJson_LoadsDefinitionsProperty()
    {
        var loader = CreateLoader();
        const string json = """
        {
          "definitions": [
            {
              "type": "test.definition",
              "id": "definition.one",
              "label": "neutral"
            }
          ]
        }
        """;

        var registry = loader.LoadJson(json);

        Assert.True(registry.Contains(DefinitionId.From("definition.one")));
    }

    [Fact]
    public void LoadJsonInto_AddsDefinitionsToExistingRegistry()
    {
        var loader = CreateLoader();
        var registry = new DefinitionRegistry();
        const string json = """
        {
          "type": "test.definition",
          "id": "definition.one",
          "label": "neutral"
        }
        """;

        loader.LoadJsonInto(json, registry);

        Assert.True(registry.Contains(DefinitionId.From("definition.one")));
    }

    [Fact]
    public void LoadFile_LoadsDefinitionsFromFile()
    {
        var loader = CreateLoader();
        var filePath = CreateTempFile("""
        {
          "type": "test.definition",
          "id": "definition.file",
          "label": "from-file"
        }
        """);

        try
        {
            var registry = loader.LoadFile(filePath);

            Assert.Equal("from-file", registry.Get<TestDefinition>(DefinitionId.From("definition.file")).Label);
        }
        finally
        {
            DeleteFile(filePath);
        }
    }

    [Fact]
    public void LoadDirectory_LoadsJsonFilesInDirectory()
    {
        var loader = CreateLoader();
        var directoryPath = CreateTempDirectory();
        var firstFile = Path.Combine(directoryPath, "a.json");
        var secondFile = Path.Combine(directoryPath, "nested", "b.json");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(secondFile)!);
            File.WriteAllText(firstFile, """
            {
              "type": "test.definition",
              "id": "definition.one",
              "label": "first"
            }
            """);
            File.WriteAllText(secondFile, """
            {
              "type": "test.definition",
              "id": "definition.two",
              "label": "second"
            }
            """);

            var registry = loader.LoadDirectory(directoryPath);

            Assert.Equal(2, registry.Definitions.Count);
            Assert.True(registry.Contains(DefinitionId.From("definition.one")));
            Assert.True(registry.Contains(DefinitionId.From("definition.two")));
        }
        finally
        {
            DeleteDirectory(directoryPath);
        }
    }

    [Fact]
    public void LoadJson_ThrowsWhenJsonIsInvalid()
    {
        var loader = CreateLoader();

        Assert.Throws<InvalidDefinitionJsonException>(() =>
            loader.LoadJson("{ invalid json }", "invalid-source"));
    }

    [Fact]
    public void LoadJson_ThrowsWhenRequiredIdIsMissing()
    {
        var loader = CreateLoader();
        const string json = """
        {
          "type": "test.definition",
          "label": "neutral"
        }
        """;

        Assert.Throws<MissingDefinitionFieldException>(() => loader.LoadJson(json));
    }

    [Fact]
    public void LoadJson_ThrowsWhenRequiredTypeIsMissing()
    {
        var loader = CreateLoader();
        const string json = """
        {
          "id": "definition.one",
          "label": "neutral"
        }
        """;

        Assert.Throws<MissingDefinitionFieldException>(() => loader.LoadJson(json));
    }

    [Fact]
    public void LoadJson_ThrowsWhenDefinitionTypeIsUnknown()
    {
        var loader = CreateLoader();
        const string json = """
        {
          "type": "unknown.definition",
          "id": "definition.one"
        }
        """;

        Assert.Throws<UnknownDefinitionTypeException>(() => loader.LoadJson(json));
    }

    [Fact]
    public void LoadJson_ThrowsWhenDefinitionIdDoesNotMatchDeserializedDefinition()
    {
        var types = new DefinitionTypeRegistry();
        types.Register<MismatchedIdDefinition>("mismatch.definition");
        var loader = new JsonDefinitionLoader(types);

        const string json = """
        {
          "type": "mismatch.definition",
          "id": "source.id"
        }
        """;

        Assert.Throws<InvalidDefinitionDataException>(() => loader.LoadJson(json));
    }

    [Fact]
    public void LoadFile_ThrowsWhenFileDoesNotExist()
    {
        var loader = CreateLoader();
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        Assert.Throws<DefinitionDataFileNotFoundException>(() => loader.LoadFile(filePath));
    }

    [Fact]
    public void LoadDirectory_ThrowsWhenDirectoryDoesNotExist()
    {
        var loader = CreateLoader();
        var directoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        Assert.Throws<DefinitionDataDirectoryNotFoundException>(() => loader.LoadDirectory(directoryPath));
    }

    [Fact]
    public void WorldState_OwnsDefinitionRegistry()
    {
        var world = new WorldState();
        var definition = new TestDefinition(DefinitionId.From("definition.one"), "neutral");

        world.Definitions.Register(definition);

        Assert.Same(definition, world.Definitions.Get<TestDefinition>(definition.Id));
    }

    [Fact]
    public void WorldState_CanUseFrozenDefinitionRegistry()
    {
        var registry = new DefinitionRegistry();
        var definition = new TestDefinition(DefinitionId.From("definition.one"), "neutral");

        registry.Register(definition);
        registry.Freeze();

        var world = new WorldState(new SimulationTime(), new EventBus(), registry, NoOpTraceLogger.Instance);

        Assert.True(world.Definitions.IsFrozen);
        Assert.Same(definition, world.Definitions.Get<TestDefinition>(definition.Id));
    }

    [Fact]
    public void LoadJson_DeserializesTypedDefinitionReferences()
    {
        var types = new DefinitionTypeRegistry();
        types.Register<TestDefinition>("test.definition");
        types.Register<ReferencingTestDefinition>("referencing.definition");
        var loader = new JsonDefinitionLoader(types);

        const string json = """
        {
          "definitions": [
            {
              "type": "test.definition",
              "id": "target.definition",
              "label": "target"
            },
            {
              "type": "referencing.definition",
              "id": "owner.definition",
              "primary": "target.definition",
              "related": [ "target.definition" ]
            }
          ]
        }
        """;

        var registry = loader.LoadJson(json);
        var owner = registry.Get<ReferencingTestDefinition>(DefinitionId.From("owner.definition"));

        Assert.Equal(DefinitionId.From("target.definition"), owner.Primary.Id);
        Assert.Single(owner.Related);
        Assert.Equal(DefinitionId.From("target.definition"), owner.Related[0].Id);
        registry.EnsureReferencesValid();
    }

    [Fact]
    public void LoadJson_LoadsReferencesThatValidationCanReject()
    {
        var types = new DefinitionTypeRegistry();
        types.Register<ReferencingTestDefinition>("referencing.definition");
        var loader = new JsonDefinitionLoader(types);

        const string json = """
        {
          "type": "referencing.definition",
          "id": "owner.definition",
          "primary": "missing.definition",
          "related": []
        }
        """;

        var registry = loader.LoadJson(json);
        var result = registry.ValidateReferences();

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    private static JsonDefinitionLoader CreateLoader()
    {
        var types = new DefinitionTypeRegistry();
        types.Register<TestDefinition>("test.definition");

        return new JsonDefinitionLoader(types);
    }

    private static string CreateTempFile(string content)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(filePath, content);
        return filePath;
    }

    private static string CreateTempDirectory()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }

    private static void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private static void DeleteDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    /// <summary>
    /// Definition that intentionally ignores the JSON identifier.
    /// </summary>
    private sealed record MismatchedIdDefinition : IDefinition
    {
        public DefinitionId Id => DefinitionId.From("different.id");
    }
}
