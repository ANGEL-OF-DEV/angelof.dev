// BootstrapToolsV0Logic.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Linq;

namespace Registry.Tool.Commands.BootstrapToolsV0;

public static class BootstrapToolsV0Logic
{
  // MONOCOQUE_PLACEHOLDER(V0): metrics: drift/complexity computation output contract
  // MONOCOQUE_PLACEHOLDER(V0): gate: semantic version bump correctness based on schema/registry diffs
  private const string RegistryRel = "[monocoque.tools]/registry/v0/registry.yaml";
  private const string ToolSchemaRel = "[monocoque.tools]/schemas/v0/tool.schema.json";
  private const string ToolRegistrySchemaRel = "[monocoque.tools]/schemas/v0/tool_registry.schema.json";
  private const string ToolDocRel = "[monocoque.tools]/tools/v0/registry.tool.yaml";

  private const string ToolSchemaId = "tool.v0";
  private const string ToolRegistrySchemaId = "tool_registry.v0";
  private const string ToolId = "registry.tool";
  private const string ToolCanonicalPath = "tool://registry.tool";
  private const string RegistryCanonicalPath = "tool-registry://tools.v0";

  private static readonly IReadOnlyList<string> Placeholders = new[]
  {
    "MONOCOQUE_PLACEHOLDER(V0): metrics: drift/complexity computation output contract",
    "MONOCOQUE_PLACEHOLDER(V0): gate: semantic version bump correctness based on schema/registry diffs"
  };

  public static OperationResult Run(string? repoRootArg)
  {
    var errors = new List<string>();
    var repoRoot = ResolveRepoRoot(repoRootArg, errors);
    if (errors.Count > 0)
      return OperationResult.Failure(errors);

    var registryPath = ResolvePath(repoRoot, RegistryRel);
    var toolSchemaPath = ResolvePath(repoRoot, ToolSchemaRel);
    var toolRegistrySchemaPath = ResolvePath(repoRoot, ToolRegistrySchemaRel);
    var toolDocPath = ResolvePath(repoRoot, ToolDocRel);

    EnsureDirectory(Path.GetDirectoryName(registryPath));
    EnsureDirectory(Path.GetDirectoryName(toolSchemaPath));
    EnsureDirectory(Path.GetDirectoryName(toolRegistrySchemaPath));
    EnsureDirectory(Path.GetDirectoryName(toolDocPath));

    var registry = LoadRegistry(registryPath, errors) ?? BuildRegistry();
    if (errors.Count > 0)
      return OperationResult.Failure(errors);

    registry = registry with
    {
      Schema = ToolRegistrySchemaId,
      CanonicalPath = RegistryCanonicalPath,
      Placeholders = Placeholders,
      Tools = MergeToolEntry(registry.Tools)
    };

    var toolDoc = BuildToolDoc();

    WriteIfChanged(toolSchemaPath, SerializeJson(BuildToolSchemaJson()));
    WriteIfChanged(toolRegistrySchemaPath, SerializeJson(BuildToolRegistrySchemaJson()));
    WriteIfChanged(toolDocPath, SerializeJson(toolDoc));
    WriteIfChanged(registryPath, SerializeJson(registry));

    return OperationResult.Success();
  }

  private static string ResolveRepoRoot(string? repoRootArg, List<string> errors)
  {
    var resolved = string.IsNullOrWhiteSpace(repoRootArg) ? "." : repoRootArg;
    var full = Path.GetFullPath(resolved);
    if (!Directory.Exists(full))
    {
      errors.Add($"repo-root not found: {repoRootArg}");
      return full;
    }

    return full;
  }

  private static string ResolvePath(string repoRoot, string relPath)
  {
    var normalized = relPath.Replace('\\', '/');
    var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
    return Path.Combine(new[] { repoRoot }.Concat(parts).ToArray());
  }

  private static ToolRegistryDoc BuildRegistry()
  {
    return new ToolRegistryDoc(
      ToolRegistrySchemaId,
      RegistryCanonicalPath,
      MergeToolEntry(Array.Empty<ToolRegistryEntry>()),
      null,
      Placeholders
    );
  }

  private static IReadOnlyList<ToolRegistryEntry> MergeToolEntry(IReadOnlyList<ToolRegistryEntry> entries)
  {
    var next = entries.Where(e => !string.Equals(e.Id, ToolId, StringComparison.OrdinalIgnoreCase)).ToList();
    next.Add(new ToolRegistryEntry(
      ToolId,
      ToolDocRel,
      "active",
      "self-hosted"
    ));

    return next.OrderBy(e => e.Id, StringComparer.OrdinalIgnoreCase).ToArray();
  }

  private static ToolDoc BuildToolDoc()
  {
    var commands = new List<ToolCommandDoc>
    {
      new("bootstrap-tools-v0", $"dotnet run --project {ToolProjectPathRel()} -c Release -- --app registry bootstrap-tools-v0 --repo-root ."),
      new("verify-tools-v0", $"dotnet run --project {ToolProjectPathRel()} -c Release -- --app registry verify-tools-v0 --repo-root .")
    };

    return new ToolDoc(
      ToolSchemaId,
      ToolId,
      ToolCanonicalPath,
      ToolProjectPathRel(),
      "dotnet",
      commands,
      null,
      Placeholders
    );
  }

  private static string ToolProjectPathRel()
  {
    return "[monocoque.tools]/Registry.Tool/Registry.Tool.csproj";
  }

  private static ToolRegistryDoc? LoadRegistry(string registryPath, List<string> errors)
  {
    if (!File.Exists(registryPath))
      return null;

    try
    {
      var json = File.ReadAllText(registryPath);
      var doc = JsonSerializer.Deserialize<ToolRegistryDoc>(json, SerializerOptions());
      if (doc is null)
      {
        errors.Add("registry.yaml is empty or invalid JSON");
        return null;
      }

      return doc;
    }
    catch (JsonException ex)
    {
      errors.Add($"registry.yaml invalid JSON: {ex.Message}");
      return null;
    }
  }

  private static void EnsureDirectory(string? path)
  {
    if (string.IsNullOrWhiteSpace(path))
      return;

    if (!Directory.Exists(path))
      Directory.CreateDirectory(path);
  }

  private static void WriteIfChanged(string path, string content)
  {
    if (File.Exists(path))
    {
      var existing = File.ReadAllText(path);
      if (string.Equals(existing, content, StringComparison.Ordinal))
        return;
    }

    File.WriteAllText(path, content);
  }

  private static string SerializeJson<T>(T value)
  {
    return JsonSerializer.Serialize(value, SerializerOptions()) + "\n";
  }

  private static JsonSerializerOptions SerializerOptions()
  {
    return new JsonSerializerOptions
    {
      WriteIndented = true,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
  }

  private static JsonObject BuildToolSchemaJson()
  {
    var schema = new JsonObject
    {
      ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
      ["$id"] = "tool.schema.v0",
      ["$comment"] = "MONOCOQUE_PLACEHOLDER(V0): metrics: drift/complexity computation output contract; MONOCOQUE_PLACEHOLDER(V0): gate: semantic version bump correctness based on schema/registry diffs",
      ["title"] = "Tool schema v0",
      ["type"] = "object",
      ["additionalProperties"] = false,
      ["required"] = new JsonArray("schema", "id", "canonical_path", "project_path_rel", "kind", "commands", "placeholders"),
      ["properties"] = new JsonObject
      {
        ["schema"] = new JsonObject { ["const"] = ToolSchemaId },
        ["id"] = new JsonObject { ["type"] = "string", ["minLength"] = 1 },
        ["canonical_path"] = new JsonObject { ["type"] = "string", ["pattern"] = "^tool://" },
        ["project_path_rel"] = new JsonObject { ["type"] = "string", ["pattern"] = "^\\[[^\\]]+\\]/" },
        ["kind"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("dotnet") },
        ["commands"] = new JsonObject
        {
          ["type"] = "array",
          ["minItems"] = 1,
          ["items"] = new JsonObject
          {
            ["type"] = "object",
            ["additionalProperties"] = false,
            ["required"] = new JsonArray("name", "example"),
            ["properties"] = new JsonObject
            {
              ["name"] = new JsonObject { ["type"] = "string", ["minLength"] = 1 },
              ["example"] = new JsonObject { ["type"] = "string", ["minLength"] = 1 }
            }
          }
        },
        ["generated_at_utc"] = new JsonObject { ["type"] = "string", ["format"] = "date-time" },
        ["placeholders"] = new JsonObject
        {
          ["type"] = "array",
          ["minItems"] = 2,
          ["items"] = new JsonObject { ["type"] = "string" }
        }
      }
    };

    return schema;
  }

  private static JsonObject BuildToolRegistrySchemaJson()
  {
    var schema = new JsonObject
    {
      ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
      ["$id"] = "tool_registry.schema.v0",
      ["$comment"] = "MONOCOQUE_PLACEHOLDER(V0): metrics: drift/complexity computation output contract; MONOCOQUE_PLACEHOLDER(V0): gate: semantic version bump correctness based on schema/registry diffs",
      ["title"] = "Tool registry schema v0",
      ["type"] = "object",
      ["additionalProperties"] = false,
      ["required"] = new JsonArray("schema", "canonical_path", "tools", "placeholders"),
      ["properties"] = new JsonObject
      {
        ["schema"] = new JsonObject { ["const"] = ToolRegistrySchemaId },
        ["canonical_path"] = new JsonObject { ["type"] = "string", ["pattern"] = "^tool-registry://" },
        ["tools"] = new JsonObject
        {
          ["type"] = "array",
          ["minItems"] = 1,
          ["items"] = new JsonObject
          {
            ["type"] = "object",
            ["additionalProperties"] = false,
            ["required"] = new JsonArray("id", "doc_path_rel", "status"),
            ["properties"] = new JsonObject
            {
              ["id"] = new JsonObject { ["type"] = "string", ["minLength"] = 1 },
              ["doc_path_rel"] = new JsonObject { ["type"] = "string", ["pattern"] = "^\\[[^\\]]+\\]/" },
              ["status"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("active", "deprecated", "inactive") },
              ["notes"] = new JsonObject { ["type"] = "string" }
            }
          }
        },
        ["generated_at_utc"] = new JsonObject { ["type"] = "string", ["format"] = "date-time" },
        ["placeholders"] = new JsonObject
        {
          ["type"] = "array",
          ["minItems"] = 2,
          ["items"] = new JsonObject { ["type"] = "string" }
        }
      }
    };

    return schema;
  }
}
