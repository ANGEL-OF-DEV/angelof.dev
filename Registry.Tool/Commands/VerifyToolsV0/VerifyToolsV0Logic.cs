// VerifyToolsV0Logic.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;
using Json.Schema;
using Registry.Tool.App.Logging;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Registry.Tool.Commands.VerifyToolsV0;

public static class VerifyToolsV0Logic
{
  private const string RegistryRel = "[monocoque.tools]/registry/v0/registry.yaml";
  private const string ToolSchemaRel = "[monocoque.tools]/schemas/v0/tool.schema.json";
  private const string ToolRegistrySchemaRel = "[monocoque.tools]/schemas/v0/tool_registry.schema.json";
  private const string AuthorityMapRel = "[monocoque.ur]/registry/v0/authority-map.v0.yaml";
  private const string AuthorityMapSchemaRel = "[monocoque.ur]/schemas/v0/authority_map.schema.json";
  private const string AuthorityMapPointerRel = "AUTHORITY-MAP.v0.md";
  private const string CommandsRootRel = "[monocoque.tools]/Registry.Tool/Commands";

  private const string ToolSchemaId = "tool.v0";
  private const string ToolRegistrySchemaId = "tool_registry.v0";

  public static CommandResult Run(VerifyOptions options)
  {
    var errors = new List<string>();
    var warnings = new List<string>();
    var decisions = new List<string>();
    var edits = new List<string>();

    var repoRoot = RepoPath.ResolveRepoRoot(options.RepoRoot, errors);
    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    using var logger = LogWriter.Create(repoRoot, options.LogOptions, errors);
    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    ValidateAuthorityMap(repoRoot, errors);

    var toolSchemaPath = RepoPath.ResolvePath(repoRoot, ToolSchemaRel);
    var toolRegistrySchemaPath = RepoPath.ResolvePath(repoRoot, ToolRegistrySchemaRel);
    var registryPath = RepoPath.ResolvePath(repoRoot, RegistryRel);

    EnsureExists(toolSchemaPath, ToolSchemaRel, errors);
    EnsureExists(toolRegistrySchemaPath, ToolRegistrySchemaRel, errors);
    EnsureExists(registryPath, RegistryRel, errors);

    if (errors.Count > 0)
    {
      Log(logger, "registry verify-tools-v0", RepoPath.NormalizeRepoRootDisplay(options.RepoRoot), decisions, edits, warnings, errors);
      return CommandResult.Failure(errors, warnings, decisions, edits);
    }

    var toolSchema = LoadSchema(toolSchemaPath, ToolSchemaRel, errors);
    var toolRegistrySchema = LoadSchema(toolRegistrySchemaPath, ToolRegistrySchemaRel, errors);

    if (errors.Count > 0 || toolSchema is null || toolRegistrySchema is null)
    {
      Log(logger, "registry verify-tools-v0", RepoPath.NormalizeRepoRootDisplay(options.RepoRoot), decisions, edits, warnings, errors);
      return CommandResult.Failure(errors, warnings, decisions, edits);
    }

    var registryYamlText = File.ReadAllText(registryPath);
    var registryDoc = DeserializeYaml<ToolRegistryDoc>(registryYamlText, RegistryRel, errors);
    var registryNode = ParseYamlAsJsonNode(registryYamlText, RegistryRel, errors);

    if (registryDoc is null || registryNode is null)
    {
      Log(logger, "registry verify-tools-v0", options.RepoRoot ?? ".", decisions, edits, warnings, errors);
      return CommandResult.Failure(errors, warnings, decisions, edits);
    }

    ValidateSchema(toolRegistrySchema, registryNode, RegistryRel, errors);
    ValidateRegistry(registryDoc, errors);

    foreach (var tool in registryDoc.Tools ?? new List<ToolRegistryEntry>())
    {
      if (!RepoPath.IsRepoRelative(tool.DocPathRel))
      {
        errors.Add($"tool doc path must be repo-relative: {tool.DocPathRel}");
        continue;
      }

      var docPath = RepoPath.ResolvePath(repoRoot, tool.DocPathRel);
      if (!File.Exists(docPath))
      {
        errors.Add($"missing tool doc: {tool.DocPathRel}");
        continue;
      }

      var docText = File.ReadAllText(docPath);
      var doc = DeserializeYaml<ToolDoc>(docText, tool.DocPathRel, errors);
      var docNode = ParseYamlAsJsonNode(docText, tool.DocPathRel, errors);

      if (doc is null || docNode is null)
        continue;

      ValidateSchema(toolSchema, docNode, tool.DocPathRel, errors);
      ValidateToolDoc(doc, tool, errors);
    }

    ValidateCommandDirectories(repoRoot, errors);

    Log(logger, "registry verify-tools-v0", RepoPath.NormalizeRepoRootDisplay(options.RepoRoot), decisions, edits, warnings, errors);

    return errors.Count == 0
      ? CommandResult.Success(warnings, decisions, edits)
      : CommandResult.Failure(errors, warnings, decisions, edits);
  }

  private static void ValidateAuthorityMap(string repoRoot, List<string> errors)
  {
    var pointerPath = RepoPath.ResolvePath(repoRoot, AuthorityMapPointerRel);
    if (!File.Exists(pointerPath))
    {
      errors.Add($"missing {AuthorityMapPointerRel}");
      return;
    }

    var pointerText = File.ReadAllText(pointerPath);
    if (!pointerText.Contains(AuthorityMapRel, StringComparison.Ordinal))
      errors.Add($"{AuthorityMapPointerRel} must reference {AuthorityMapRel}");

    var mapPath = RepoPath.ResolvePath(repoRoot, AuthorityMapRel);
    if (!File.Exists(mapPath))
    {
      errors.Add($"missing authority map: {AuthorityMapRel}");
      return;
    }

    var mapText = File.ReadAllText(mapPath);
    var mapNode = ParseYamlAsJsonNode(mapText, AuthorityMapRel, errors);

    var schemaPath = RepoPath.ResolvePath(repoRoot, AuthorityMapSchemaRel);
    if (!File.Exists(schemaPath))
    {
      errors.Add($"missing authority map schema: {AuthorityMapSchemaRel}");
      return;
    }

    var schema = LoadSchema(schemaPath, AuthorityMapSchemaRel, errors);
    if (schema is null || mapNode is null)
      return;

    ValidateSchema(schema, mapNode, AuthorityMapRel, errors);
  }

  private static void ValidateRegistry(ToolRegistryDoc registry, List<string> errors)
  {
    if (!string.Equals(registry.Schema, ToolRegistrySchemaId, StringComparison.Ordinal))
      errors.Add($"registry schema must be '{ToolRegistrySchemaId}'");

    if (!registry.CanonicalPath.StartsWith("tool-registry://", StringComparison.Ordinal))
      errors.Add("registry canonical_path must start with tool-registry://");

    if (registry.Tools is null || registry.Tools.Count == 0)
      errors.Add("registry must contain at least one tool entry");
  }

  private static void ValidateToolDoc(ToolDoc doc, ToolRegistryEntry registryEntry, List<string> errors)
  {
    if (string.IsNullOrWhiteSpace(doc.Schema))
    {
      errors.Add($"tool schema is required for {registryEntry.DocPathRel}");
    }
    else if (!string.Equals(doc.Schema, ToolSchemaId, StringComparison.Ordinal))
      errors.Add($"tool schema must be '{ToolSchemaId}' for {registryEntry.DocPathRel}");

    if (string.IsNullOrWhiteSpace(doc.Id))
      errors.Add($"tool id is required for {registryEntry.DocPathRel}");
    else if (!string.Equals(doc.Id, registryEntry.Id, StringComparison.Ordinal))
      errors.Add($"tool id mismatch: registry {registryEntry.Id} vs doc {doc.Id}");

    if (string.IsNullOrWhiteSpace(doc.CanonicalPath))
      errors.Add($"tool canonical_path is required for {registryEntry.DocPathRel}");
    else if (!doc.CanonicalPath.StartsWith("tool://", StringComparison.Ordinal))
      errors.Add($"tool canonical_path must start with tool://: {doc.CanonicalPath}");

    if (string.IsNullOrWhiteSpace(doc.ProjectPathRel))
      errors.Add($"project_path_rel is required for {registryEntry.DocPathRel}");
    else if (!RepoPath.IsRepoRelative(doc.ProjectPathRel))
      errors.Add($"project_path_rel must be repo-relative: {doc.ProjectPathRel}");

    if (string.IsNullOrWhiteSpace(doc.Kind))
      errors.Add($"tool kind is required for {registryEntry.DocPathRel}");
    else if (!string.Equals(doc.Kind, "dotnet", StringComparison.Ordinal))
      errors.Add($"tool kind must be dotnet: {doc.Kind}");

    if (doc.Commands is null || doc.Commands.Count == 0)
      errors.Add("tool commands must not be empty");

    foreach (var cmd in doc.Commands ?? new List<ToolCommandDoc>())
    {
      if (string.IsNullOrWhiteSpace(cmd.Name))
        errors.Add("tool command name is required");

      if (string.IsNullOrWhiteSpace(cmd.Example))
      {
        errors.Add("tool command example is required");
        continue;
      }

      if (!cmd.Example.Contains("--repo-root .", StringComparison.Ordinal))
        errors.Add($"tool command example must include --repo-root .: {cmd.Name}");

      if (!cmd.Example.Contains(doc.ProjectPathRel, StringComparison.Ordinal))
        errors.Add($"tool command example must include project_path_rel: {cmd.Name}");
    }
  }

  private static void ValidateCommandDirectories(string repoRoot, List<string> errors)
  {
    var commandsRoot = RepoPath.ResolvePath(repoRoot, CommandsRootRel);
    if (!Directory.Exists(commandsRoot))
    {
      errors.Add($"missing commands directory: {CommandsRootRel}");
      return;
    }

    foreach (var directory in Directory.EnumerateDirectories(commandsRoot))
    {
      var csFiles = Directory.EnumerateFiles(directory, "*.cs", SearchOption.TopDirectoryOnly).ToList();
      var testFiles = csFiles.Where(f => f.EndsWith("Tests.cs", StringComparison.Ordinal)).ToList();
      var nonTestFiles = csFiles.Where(f => !f.EndsWith("Tests.cs", StringComparison.Ordinal)).ToList();

      var rel = RepoRelative(repoRoot, directory);
      if (testFiles.Count != 1)
        errors.Add($"{rel}: expected exactly one *Tests.cs, found {testFiles.Count}");

      if (nonTestFiles.Count == 0)
        errors.Add($"{rel}: expected at least one non-test .cs file");
    }
  }

  private static void EnsureExists(string path, string label, List<string> errors)
  {
    if (!File.Exists(path))
      errors.Add($"missing {label}");
  }

  private static JsonSchema? LoadSchema(string path, string label, List<string> errors)
  {
    try
    {
      var json = File.ReadAllText(path);
      return JsonSchema.FromText(json);
    }
    catch (Exception ex)
    {
      errors.Add($"invalid JSON schema {label}: {ex.Message}");
      return null;
    }
  }

  private static void ValidateSchema(JsonSchema schema, JsonNode instance, string label, List<string> errors)
  {
    var results = schema.Evaluate(instance);
    if (!results.IsValid)
      errors.Add($"schema validation failed for {label}");
  }

  private static T? DeserializeYaml<T>(string yaml, string label, List<string> errors)
  {
    try
    {
      var deserializer = BuildDeserializer();
      var value = deserializer.Deserialize<T>(yaml);
      if (value is null)
        errors.Add($"{label} is empty or invalid");

      return value;
    }
    catch (YamlException ex)
    {
      errors.Add($"invalid YAML in {label}: {ex.Message}");
      return default;
    }
  }

  private static JsonNode? ParseYamlAsJsonNode(string yaml, string label, List<string> errors)
  {
    try
    {
      var deserializer = BuildDeserializer();
      var data = deserializer.Deserialize<object>(yaml);
      if (data is null)
      {
        errors.Add($"{label} is empty or invalid");
        return null;
      }
      return ToJsonNode(data);
    }
    catch (YamlException ex)
    {
      errors.Add($"invalid YAML in {label}: {ex.Message}");
      return null;
    }
  }

  private static JsonNode? ToJsonNode(object? value)
  {
    switch (value)
    {
      case null:
        return null;
      case string s:
        return JsonValue.Create(s);
      case bool b:
        return JsonValue.Create(b);
      case int i:
        return JsonValue.Create(i);
      case long l:
        return JsonValue.Create(l);
      case double d:
        return JsonValue.Create(d);
      case float f:
        return JsonValue.Create(f);
      case decimal dec:
        return JsonValue.Create(dec);
      case IDictionary<object, object> dict:
      {
        var obj = new JsonObject();
        foreach (var entry in dict)
        {
          var key = entry.Key?.ToString() ?? string.Empty;
          obj[key] = ToJsonNode(entry.Value);
        }
        return obj;
      }
      case IDictionary<string, object> dictString:
      {
        var obj = new JsonObject();
        foreach (var entry in dictString)
          obj[entry.Key] = ToJsonNode(entry.Value);
        return obj;
      }
      case IEnumerable<object> list:
      {
        var array = new JsonArray();
        foreach (var item in list)
          array.Add(ToJsonNode(item));
        return array;
      }
      default:
        return JsonValue.Create(value.ToString());
    }
  }

  private static IDeserializer BuildDeserializer()
  {
    return new DeserializerBuilder()
      .WithNamingConvention(NullNamingConvention.Instance)
      .IgnoreUnmatchedProperties()
      .Build();
  }

  private static string RepoRelative(string repoRoot, string fullPath)
  {
    var rel = Path.GetRelativePath(repoRoot, fullPath).Replace('\\', '/');
    return rel;
  }

  private static void Log(
    LogWriter? logger,
    string command,
    string repoRootDisplay,
    IReadOnlyList<string> decisions,
    IReadOnlyList<string> edits,
    IReadOnlyList<string> warnings,
    IReadOnlyList<string> errors)
  {
    if (logger is null)
      return;

    logger.Write(new LogEntry(
      DateTimeOffset.UtcNow.ToString("O"),
      command,
      repoRootDisplay,
      decisions,
      edits,
      warnings,
      errors.Count == 0 ? null : errors
    ));
  }
}
