// BootstrapToolsV0Logic.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Text.Json;
using System.Linq;
using Registry.Tool.App.Logging;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Registry.Tool.Commands.BootstrapToolsV0;

public static class BootstrapToolsV0Logic
{
  private const string RegistryRel = "[monocoque.tools]/registry/v0/registry.yaml";
  private const string ToolSchemaRel = "[monocoque.tools]/schemas/v0/tool.schema.json";
  private const string ToolRegistrySchemaRel = "[monocoque.tools]/schemas/v0/tool_registry.schema.json";
  private const string ToolDocRel = "[monocoque.tools]/tools/v0/registry.tool.yaml";

  private const string UrRegistryRel = "[monocoque.ur]/registry/v0/registry.yaml";
  private const string TemplatePackDefaultRel = "[monocoque.ur]/templates/registry-tool/v0/pack.yaml";
  private const string TemplatePackEntryId = "TEMPLATE.REGISTRY.TOOL.v0";

  private const string ToolSchemaId = "tool.v0";
  private const string ToolRegistrySchemaId = "tool_registry.v0";
  private const string ToolId = "registry.tool";
  private const string ToolCanonicalPath = "tool://registry.tool";
  private const string RegistryCanonicalPath = "tool-registry://tools.v0";

  public static CommandResult Run(BootstrapOptions options)
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

    var templates = LoadTemplates(repoRoot, warnings, decisions, errors);
    if (templates is null)
    {
      Log(logger, "registry bootstrap-tools-v0", RepoPath.NormalizeRepoRootDisplay(options.RepoRoot), decisions, edits, warnings, errors);
      return CommandResult.Failure(errors, warnings, decisions, edits);
    }

    var tokens = new Dictionary<string, string>(StringComparer.Ordinal)
    {
      ["TOOL_ID"] = ToolId,
      ["TOOL_CANONICAL_PATH"] = ToolCanonicalPath,
      ["TOOL_PROJECT_PATH_REL"] = ToolProjectPathRel(),
      ["BOOTSTRAP_COMMAND"] = $"dotnet run --project {ToolProjectPathRel()} -c Release -- --app registry bootstrap-tools-v0 --repo-root .",
      ["VERIFY_COMMAND"] = $"dotnet run --project {ToolProjectPathRel()} -c Release -- --app registry verify-tools-v0 --repo-root ."
    };

    var toolDocText = RenderTemplate(templates.ToolDocYaml, tokens);
    var toolDoc = DeserializeYaml<ToolDoc>(toolDocText, "tool.doc.yaml", errors);
    if (toolDoc is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var registryTemplate = DeserializeYaml<ToolRegistryDoc>(templates.RegistryYaml, "tool.registry.yaml", errors);
    if (registryTemplate is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var registryPath = RepoPath.ResolvePath(repoRoot, RegistryRel);
    var existingRegistry = LoadRegistry(registryPath, errors);
    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var registryDoc = existingRegistry ?? registryTemplate;
    registryDoc.Schema = ToolRegistrySchemaId;
    registryDoc.CanonicalPath = RegistryCanonicalPath;
    registryDoc.Placeholders = registryTemplate.Placeholders ?? new List<string>();
    registryDoc.Tools = MergeToolEntry(existingRegistry?.Tools ?? registryTemplate.Tools);
    registryDoc.GeneratedAtUtc = existingRegistry?.GeneratedAtUtc ?? registryTemplate.GeneratedAtUtc;

    var toolDocYaml = SerializeYaml(toolDoc);
    var registryYaml = SerializeYaml(registryDoc);

    var toolSchemaPath = RepoPath.ResolvePath(repoRoot, ToolSchemaRel);
    var toolRegistrySchemaPath = RepoPath.ResolvePath(repoRoot, ToolRegistrySchemaRel);
    var toolDocPath = RepoPath.ResolvePath(repoRoot, ToolDocRel);

    EnsureDirectory(Path.GetDirectoryName(registryPath));
    EnsureDirectory(Path.GetDirectoryName(toolSchemaPath));
    EnsureDirectory(Path.GetDirectoryName(toolRegistrySchemaPath));
    EnsureDirectory(Path.GetDirectoryName(toolDocPath));

    if (!WriteSchemaIfAllowed(toolSchemaPath, templates.ToolSchemaJson, ToolSchemaRel, options.Force, errors, edits))
      return CommandResult.Failure(errors, warnings, decisions, edits);

    if (!WriteSchemaIfAllowed(toolRegistrySchemaPath, templates.ToolRegistrySchemaJson, ToolRegistrySchemaRel, options.Force, errors, edits))
      return CommandResult.Failure(errors, warnings, decisions, edits);

    WriteIfChanged(toolDocPath, toolDocYaml, ToolDocRel, edits);
    WriteIfChanged(registryPath, registryYaml, RegistryRel, edits);

    Log(logger, "registry bootstrap-tools-v0", RepoPath.NormalizeRepoRootDisplay(options.RepoRoot), decisions, edits, warnings, errors);

    return errors.Count == 0
      ? CommandResult.Success(warnings, decisions, edits)
      : CommandResult.Failure(errors, warnings, decisions, edits);
  }

  private static TemplateBundle? LoadTemplates(
    string repoRoot,
    List<string> warnings,
    List<string> decisions,
    List<string> errors)
  {
    var packPath = ResolveTemplatePackPath(repoRoot, warnings, decisions, errors);
    if (errors.Count > 0)
      return null;

    if (!File.Exists(packPath))
    {
      errors.Add($"template pack not found: {RepoPath.ToRepoRelative(repoRoot, packPath)}");
      return null;
    }

    var packText = File.ReadAllText(packPath);
    var pack = DeserializeYaml<TemplatePack>(packText, "template pack", errors);
    if (pack is null)
      return null;

    var templates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var entry in pack.Templates)
    {
      if (!RepoPath.IsRepoRelative(entry.PathRel))
      {
        errors.Add($"template path must be repo-relative: {entry.PathRel}");
        continue;
      }

      var path = RepoPath.ResolvePath(repoRoot, entry.PathRel);
      if (!File.Exists(path))
      {
        errors.Add($"template file missing: {entry.PathRel}");
        continue;
      }

      templates[entry.Id] = File.ReadAllText(path);
    }

    if (errors.Count > 0)
      return null;

    var bundle = new TemplateBundle(
      GetTemplate(templates, "tool.schema.json", errors),
      GetTemplate(templates, "tool_registry.schema.json", errors),
      GetTemplate(templates, "tool.doc.yaml", errors),
      GetTemplate(templates, "tool.registry.yaml", errors)
    );

    return errors.Count > 0 ? null : bundle;
  }

  private static string ResolveTemplatePackPath(
    string repoRoot,
    List<string> warnings,
    List<string> decisions,
    List<string> errors)
  {
    var urRegistryPath = RepoPath.ResolvePath(repoRoot, UrRegistryRel);
    if (File.Exists(urRegistryPath))
    {
      if (TryResolveTemplatePackFromUrRegistry(urRegistryPath, repoRoot, decisions, errors, out var resolved))
        return resolved;

      if (errors.Count > 0)
        return RepoPath.ResolvePath(repoRoot, TemplatePackDefaultRel);
    }

    warnings.Add("template-pack: falling back to default pack path");
    decisions.Add("template-pack: fallback-default");
    // MONOCOQUE_PLACEHOLDER(V0): remove bootstrap hardcode; resolve templates via registry only
    return RepoPath.ResolvePath(repoRoot, TemplatePackDefaultRel);
  }

  private static bool TryResolveTemplatePackFromUrRegistry(
    string urRegistryPath,
    string repoRoot,
    List<string> decisions,
    List<string> errors,
    out string resolvedPath)
  {
    resolvedPath = string.Empty;

    try
    {
      var yaml = new YamlStream();
      using var reader = new StringReader(File.ReadAllText(urRegistryPath));
      yaml.Load(reader);

      if (yaml.Documents.Count == 0)
        return false;

      if (yaml.Documents[0].RootNode is not YamlMappingNode root)
        return false;

      if (!root.Children.TryGetValue(new YamlScalarNode("entries"), out var entriesNode))
        return false;

      if (entriesNode is not YamlSequenceNode entries)
        return false;

      foreach (var entryNode in entries.Children)
      {
        if (entryNode is not YamlMappingNode entry)
          continue;

        var id = GetScalar(entry, "id");
        var kind = GetScalar(entry, "kind");
        var path = GetScalar(entry, "path");

        if (!string.Equals(kind, "template_pack", StringComparison.OrdinalIgnoreCase))
          continue;

        if (!string.Equals(id, TemplatePackEntryId, StringComparison.OrdinalIgnoreCase))
          continue;

        if (string.IsNullOrWhiteSpace(path))
          return false;

        if (path.StartsWith("ur/", StringComparison.Ordinal))
        {
          var relative = path.Substring("ur/".Length);
          decisions.Add($"template-pack: ur-registry {id} -> {path}");
          resolvedPath = Path.Combine(repoRoot, "[monocoque.ur]", relative.Replace('/', Path.DirectorySeparatorChar));
          return true;
        }
      }

      return false;
    }
    catch (YamlException ex)
    {
      errors.Add($"invalid YAML in ur registry: {ex.Message}");
      return false;
    }
  }

  private static string? GetScalar(YamlMappingNode mapping, string key)
  {
    if (!mapping.Children.TryGetValue(new YamlScalarNode(key), out var node))
      return null;

    return node is YamlScalarNode scalar ? scalar.Value : null;
  }

  private static ToolRegistryDoc? LoadRegistry(string registryPath, List<string> errors)
  {
    if (!File.Exists(registryPath))
      return null;

    try
    {
      var text = File.ReadAllText(registryPath);
      return DeserializeYaml<ToolRegistryDoc>(text, "registry.yaml", errors);
    }
    catch (Exception ex)
    {
      errors.Add($"registry.yaml read failed: {ex.Message}");
      return null;
    }
  }

  private static List<ToolRegistryEntry> MergeToolEntry(List<ToolRegistryEntry>? entries)
  {
    var list = entries?.Where(e => !string.Equals(e.Id, ToolId, StringComparison.OrdinalIgnoreCase)).ToList()
      ?? new List<ToolRegistryEntry>();

    list.Add(new ToolRegistryEntry
    {
      Id = ToolId,
      DocPathRel = ToolDocRel,
      Status = "active",
      Notes = "self-hosted"
    });

    return list.OrderBy(e => e.Id, StringComparer.OrdinalIgnoreCase).ToList();
  }

  private static string ToolProjectPathRel()
  {
    return "[monocoque.tools]/Registry.Tool/Registry.Tool.csproj";
  }

  private static void EnsureDirectory(string? path)
  {
    if (string.IsNullOrWhiteSpace(path))
      return;

    if (!Directory.Exists(path))
      Directory.CreateDirectory(path);
  }

  private static string RenderTemplate(string template, IReadOnlyDictionary<string, string> tokens)
  {
    var output = template;
    foreach (var token in tokens)
    {
      output = output.Replace("{{" + token.Key + "}}", token.Value, StringComparison.Ordinal);
    }

    return output;
  }

  private static bool WriteSchemaIfAllowed(
    string path,
    string template,
    string relPath,
    bool force,
    List<string> errors,
    List<string> edits)
  {
    if (!IsJsonValid(template, relPath, errors))
      return false;

    var normalized = EnsureTrailingNewline(template);
    if (File.Exists(path))
    {
      var existing = File.ReadAllText(path);
      if (!string.Equals(existing, normalized, StringComparison.Ordinal))
      {
        if (!force)
        {
          errors.Add($"schema differs for {relPath}; use --force to overwrite");
          return false;
        }

        File.WriteAllText(path, normalized);
        edits.Add($"updated {relPath} (force)");
        return true;
      }

      return true;
    }

    File.WriteAllText(path, normalized);
    edits.Add($"created {relPath}");
    return true;
  }

  private static void WriteIfChanged(string path, string content, string relPath, List<string> edits)
  {
    var normalized = EnsureTrailingNewline(content);
    if (File.Exists(path))
    {
      var existing = File.ReadAllText(path);
      if (string.Equals(existing, normalized, StringComparison.Ordinal))
        return;
    }

    File.WriteAllText(path, normalized);
    edits.Add($"wrote {relPath}");
  }

  private static bool IsJsonValid(string json, string label, List<string> errors)
  {
    try
    {
      using var _ = JsonDocument.Parse(json);
      return true;
    }
    catch (JsonException ex)
    {
      errors.Add($"invalid JSON template for {label}: {ex.Message}");
      return false;
    }
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

  private static string SerializeYaml<T>(T value)
  {
    var serializer = BuildSerializer();
    return serializer.Serialize(value);
  }

  private static ISerializer BuildSerializer()
  {
    return new SerializerBuilder()
      .WithNamingConvention(NullNamingConvention.Instance)
      .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
      .DisableAliases()
      .Build();
  }

  private static IDeserializer BuildDeserializer()
  {
    return new DeserializerBuilder()
      .WithNamingConvention(NullNamingConvention.Instance)
      .IgnoreUnmatchedProperties()
      .Build();
  }

  private static string EnsureTrailingNewline(string content)
  {
    return content.EndsWith("\n", StringComparison.Ordinal) ? content : content + "\n";
  }

  private static string GetTemplate(Dictionary<string, string> templates, string id, List<string> errors)
  {
    if (templates.TryGetValue(id, out var template))
      return template;

    errors.Add($"template '{id}' not found in pack");
    return string.Empty;
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

  private sealed record TemplateBundle(
    string ToolSchemaJson,
    string ToolRegistrySchemaJson,
    string ToolDocYaml,
    string RegistryYaml
  );
}
