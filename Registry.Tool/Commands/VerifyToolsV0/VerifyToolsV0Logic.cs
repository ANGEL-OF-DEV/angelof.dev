// VerifyToolsV0Logic.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Text.Json;

namespace Registry.Tool.Commands.VerifyToolsV0;

public static class VerifyToolsV0Logic
{
  // MONOCOQUE_PLACEHOLDER(V0): metrics: drift/complexity computation output contract
  // MONOCOQUE_PLACEHOLDER(V0): gate: semantic version bump correctness based on schema/registry diffs
  private const string RegistryRel = "[monocoque.tools]/registry/v0/registry.yaml";
  private const string ToolSchemaRel = "[monocoque.tools]/schemas/v0/tool.schema.json";
  private const string ToolRegistrySchemaRel = "[monocoque.tools]/schemas/v0/tool_registry.schema.json";

  private const string ToolSchemaId = "tool.v0";
  private const string ToolRegistrySchemaId = "tool_registry.v0";

  private static readonly IReadOnlyList<string> Placeholders = new[]
  {
    "MONOCOQUE_PLACEHOLDER(V0): metrics: drift/complexity computation output contract",
    "MONOCOQUE_PLACEHOLDER(V0): gate: semantic version bump correctness based on schema/registry diffs"
  };

  public static OperationResult Run(string? repoRootArg)
  {
    var errors = new List<string>();
    var repoRoot = ResolveRepoRoot(repoRootArg, errors);

    var toolSchemaPath = ResolvePath(repoRoot, ToolSchemaRel);
    var toolRegistrySchemaPath = ResolvePath(repoRoot, ToolRegistrySchemaRel);
    var registryPath = ResolvePath(repoRoot, RegistryRel);

    EnsureExists(toolSchemaPath, "tool.schema.json", errors);
    EnsureExists(toolRegistrySchemaPath, "tool_registry.schema.json", errors);
    EnsureExists(registryPath, "registry.yaml", errors);

    if (errors.Count > 0)
      return OperationResult.Failure(errors);

    if (!ValidateJsonFile(toolSchemaPath, "tool.schema.json", errors))
      return OperationResult.Failure(errors);

    if (!ValidateJsonFile(toolRegistrySchemaPath, "tool_registry.schema.json", errors))
      return OperationResult.Failure(errors);

    var registry = ParseRegistry(registryPath, errors);
    if (registry is null)
      return OperationResult.Failure(errors);

    ValidateRegistry(registry, errors);
    if (errors.Count > 0)
      return OperationResult.Failure(errors);

    foreach (var tool in registry.Tools)
    {
      if (!IsRepoRelativePath(tool.DocPathRel))
      {
        errors.Add($"tool doc path must be repo-relative: {tool.DocPathRel}");
        continue;
      }

      var docPath = ResolvePath(repoRoot, tool.DocPathRel);
      if (!File.Exists(docPath))
      {
        errors.Add($"missing tool doc: {tool.DocPathRel}");
        continue;
      }

      var doc = ParseToolDoc(docPath, errors);
      if (doc is null)
        continue;

      ValidateToolDoc(doc, tool, errors);
    }

    return errors.Count == 0 ? OperationResult.Success() : OperationResult.Failure(errors);
  }

  private static string ResolveRepoRoot(string? repoRootArg, List<string> errors)
  {
    var resolved = string.IsNullOrWhiteSpace(repoRootArg) ? "." : repoRootArg;
    var full = Path.GetFullPath(resolved);
    if (!Directory.Exists(full))
      errors.Add($"repo-root not found: {repoRootArg}");

    return full;
  }

  private static void EnsureExists(string path, string label, List<string> errors)
  {
    if (!File.Exists(path))
      errors.Add($"missing {label}: {path}");
  }

  private static bool ValidateJsonFile(string path, string label, List<string> errors)
  {
    try
    {
      using var _ = JsonDocument.Parse(File.ReadAllText(path));
      return true;
    }
    catch (JsonException ex)
    {
      errors.Add($"invalid JSON in {label}: {ex.Message}");
      return false;
    }
  }

  private static RegistryParsed? ParseRegistry(string registryPath, List<string> errors)
  {
    try
    {
      using var doc = JsonDocument.Parse(File.ReadAllText(registryPath));
      var root = doc.RootElement;
      if (root.ValueKind != JsonValueKind.Object)
      {
        errors.Add("registry.yaml must be a JSON object");
        return null;
      }

      var schema = GetRequiredString(root, "schema", "registry.yaml", errors);
      var canonical = GetRequiredString(root, "canonical_path", "registry.yaml", errors);
      var placeholders = GetRequiredStringArray(root, "placeholders", "registry.yaml", errors);
      var tools = GetRequiredArray(root, "tools", "registry.yaml", errors);

      if (schema is null || canonical is null || placeholders is null || tools is null)
        return null;

      var entries = new List<ToolRegistryEntryParsed>();
      foreach (var item in tools.Value.EnumerateArray())
      {
        if (item.ValueKind != JsonValueKind.Object)
        {
          errors.Add("registry.yaml tools entries must be objects");
          continue;
        }

        var id = GetRequiredString(item, "id", "registry.yaml", errors);
        var docPath = GetRequiredString(item, "doc_path_rel", "registry.yaml", errors);
        var status = GetRequiredString(item, "status", "registry.yaml", errors);
        var notes = GetOptionalString(item, "notes");

        if (id is null || docPath is null || status is null)
          continue;

        entries.Add(new ToolRegistryEntryParsed(id, docPath, status, notes));
      }

      return new RegistryParsed(schema, canonical, entries, placeholders);
    }
    catch (JsonException ex)
    {
      errors.Add($"registry.yaml invalid JSON: {ex.Message}");
      return null;
    }
  }

  private static ToolDocParsed? ParseToolDoc(string docPath, List<string> errors)
  {
    try
    {
      using var doc = JsonDocument.Parse(File.ReadAllText(docPath));
      var root = doc.RootElement;
      if (root.ValueKind != JsonValueKind.Object)
      {
        errors.Add($"tool doc must be a JSON object: {docPath}");
        return null;
      }

      var schema = GetRequiredString(root, "schema", docPath, errors);
      var id = GetRequiredString(root, "id", docPath, errors);
      var canonical = GetRequiredString(root, "canonical_path", docPath, errors);
      var projectPath = GetRequiredString(root, "project_path_rel", docPath, errors);
      var kind = GetRequiredString(root, "kind", docPath, errors);
      var placeholders = GetRequiredStringArray(root, "placeholders", docPath, errors);
      var commandsArray = GetRequiredArray(root, "commands", docPath, errors);

      if (schema is null || id is null || canonical is null || projectPath is null || kind is null || placeholders is null || commandsArray is null)
        return null;

      var commands = new List<ToolCommandParsed>();
      foreach (var item in commandsArray.Value.EnumerateArray())
      {
        if (item.ValueKind != JsonValueKind.Object)
        {
          errors.Add($"commands entries must be objects: {docPath}");
          continue;
        }

        var name = GetRequiredString(item, "name", docPath, errors);
        var example = GetRequiredString(item, "example", docPath, errors);
        if (name is null || example is null)
          continue;

        commands.Add(new ToolCommandParsed(name, example));
      }

      return new ToolDocParsed(schema, id, canonical, projectPath, kind, commands, placeholders);
    }
    catch (JsonException ex)
    {
      errors.Add($"tool doc invalid JSON: {docPath}: {ex.Message}");
      return null;
    }
  }

  private static void ValidateRegistry(RegistryParsed registry, List<string> errors)
  {
    if (!string.Equals(registry.Schema, ToolRegistrySchemaId, StringComparison.Ordinal))
      errors.Add($"registry schema must be '{ToolRegistrySchemaId}'");

    if (!registry.CanonicalPath.StartsWith("tool-registry://", StringComparison.Ordinal))
      errors.Add("registry canonical_path must start with tool-registry://");

    if (registry.Tools.Count == 0)
      errors.Add("registry must contain at least one tool entry");

    foreach (var placeholder in Placeholders)
    {
      if (!registry.Placeholders.Contains(placeholder, StringComparer.Ordinal))
        errors.Add($"registry missing placeholder: {placeholder}");
    }

    foreach (var tool in registry.Tools)
    {
      if (string.IsNullOrWhiteSpace(tool.Id))
        errors.Add("registry tool id is required");

      if (!IsRepoRelativePath(tool.DocPathRel))
        errors.Add($"tool doc path must be repo-relative: {tool.DocPathRel}");

      if (!IsStatusValid(tool.Status))
        errors.Add($"tool status invalid: {tool.Status}");
    }
  }

  private static void ValidateToolDoc(ToolDocParsed doc, ToolRegistryEntryParsed registryEntry, List<string> errors)
  {
    if (!string.Equals(doc.Schema, ToolSchemaId, StringComparison.Ordinal))
      errors.Add($"tool schema must be '{ToolSchemaId}' for {registryEntry.DocPathRel}");

    if (!string.Equals(doc.Id, registryEntry.Id, StringComparison.Ordinal))
      errors.Add($"tool id mismatch: registry {registryEntry.Id} vs doc {doc.Id}");

    if (!doc.CanonicalPath.StartsWith("tool://", StringComparison.Ordinal))
      errors.Add($"tool canonical_path must start with tool://: {doc.CanonicalPath}");

    if (!IsRepoRelativePath(doc.ProjectPathRel))
      errors.Add($"project_path_rel must be repo-relative: {doc.ProjectPathRel}");

    if (!string.Equals(doc.Kind, "dotnet", StringComparison.Ordinal))
      errors.Add($"tool kind must be dotnet: {doc.Kind}");

    if (doc.Commands.Count == 0)
      errors.Add("tool commands must not be empty");

    foreach (var cmd in doc.Commands)
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

    foreach (var placeholder in Placeholders)
    {
      if (!doc.Placeholders.Contains(placeholder, StringComparer.Ordinal))
        errors.Add($"tool doc missing placeholder: {placeholder}");
    }
  }

  private static bool IsStatusValid(string status)
  {
    return string.Equals(status, "active", StringComparison.Ordinal)
      || string.Equals(status, "deprecated", StringComparison.Ordinal)
      || string.Equals(status, "inactive", StringComparison.Ordinal);
  }

  private static bool IsRepoRelativePath(string path)
  {
    if (string.IsNullOrWhiteSpace(path))
      return false;

    if (Path.IsPathRooted(path))
      return false;

    if (path.Contains(":", StringComparison.Ordinal))
      return false;

    if (path.Contains("..", StringComparison.Ordinal))
      return false;

    return path.StartsWith("[", StringComparison.Ordinal);
  }

  private static string ResolvePath(string repoRoot, string relPath)
  {
    var normalized = relPath.Replace('\\', '/');
    var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
    return Path.Combine(new[] { repoRoot }.Concat(parts).ToArray());
  }

  private static string? GetRequiredString(JsonElement root, string name, string source, List<string> errors)
  {
    if (!root.TryGetProperty(name, out var value))
    {
      errors.Add($"missing '{name}' in {source}");
      return null;
    }

    if (value.ValueKind != JsonValueKind.String)
    {
      errors.Add($"'{name}' must be a string in {source}");
      return null;
    }

    var text = value.GetString() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(text))
    {
      errors.Add($"'{name}' must not be empty in {source}");
      return null;
    }

    return text;
  }

  private static string? GetOptionalString(JsonElement root, string name)
  {
    if (!root.TryGetProperty(name, out var value))
      return null;

    return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
  }

  private static IReadOnlyList<string>? GetRequiredStringArray(JsonElement root, string name, string source, List<string> errors)
  {
    var array = GetRequiredArray(root, name, source, errors);
    if (array is null)
      return null;

    var list = new List<string>();
    foreach (var item in array.Value.EnumerateArray())
    {
      if (item.ValueKind != JsonValueKind.String)
      {
        errors.Add($"'{name}' must contain only strings in {source}");
        return null;
      }

      var value = item.GetString();
      if (string.IsNullOrWhiteSpace(value))
      {
        errors.Add($"'{name}' must not contain empty strings in {source}");
        return null;
      }

      list.Add(value);
    }

    return list;
  }

  private static JsonElement? GetRequiredArray(JsonElement root, string name, string source, List<string> errors)
  {
    if (!root.TryGetProperty(name, out var value))
    {
      errors.Add($"missing '{name}' in {source}");
      return null;
    }

    if (value.ValueKind != JsonValueKind.Array)
    {
      errors.Add($"'{name}' must be an array in {source}");
      return null;
    }

    return value;
  }
}
