// VerifyToolsV0Models.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using Registry.Tool.App.Logging;

namespace Registry.Tool.Commands.VerifyToolsV0;

public sealed record VerifyOptions(
  string? RepoRoot,
  LogOptions LogOptions);

public sealed record CommandResult(
  bool Ok,
  IReadOnlyList<string> Errors,
  IReadOnlyList<string> Warnings,
  IReadOnlyList<string> Decisions,
  IReadOnlyList<string> Edits)
{
  public static CommandResult Success(
    IReadOnlyList<string> warnings,
    IReadOnlyList<string> decisions,
    IReadOnlyList<string> edits)
    => new(true, Array.Empty<string>(), warnings, decisions, edits);

  public static CommandResult Failure(
    IReadOnlyList<string> errors,
    IReadOnlyList<string> warnings,
    IReadOnlyList<string> decisions,
    IReadOnlyList<string> edits)
    => new(false, errors, warnings, decisions, edits);
}

public sealed class ToolCommandDoc
{
  [YamlMember(Alias = "name", Order = 1)]
  public string Name { get; set; } = string.Empty;

  [YamlMember(Alias = "example", Order = 2)]
  public string Example { get; set; } = string.Empty;
}

public sealed class ToolDoc
{
  [YamlMember(Alias = "schema", Order = 1)]
  public string Schema { get; set; } = string.Empty;

  [YamlMember(Alias = "id", Order = 2)]
  public string Id { get; set; } = string.Empty;

  [YamlMember(Alias = "canonical_path", Order = 3)]
  public string CanonicalPath { get; set; } = string.Empty;

  [YamlMember(Alias = "project_path_rel", Order = 4, ScalarStyle = ScalarStyle.DoubleQuoted)]
  public string ProjectPathRel { get; set; } = string.Empty;

  [YamlMember(Alias = "kind", Order = 5)]
  public string Kind { get; set; } = string.Empty;

  [YamlMember(Alias = "commands", Order = 6)]
  public List<ToolCommandDoc> Commands { get; set; } = new();

  [YamlMember(Alias = "generated_at_utc", Order = 7, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
  public string? GeneratedAtUtc { get; set; }

  [YamlMember(Alias = "placeholders", Order = 8)]
  public List<string> Placeholders { get; set; } = new();
}

public sealed class ToolRegistryEntry
{
  [YamlMember(Alias = "id", Order = 1)]
  public string Id { get; set; } = string.Empty;

  [YamlMember(Alias = "doc_path_rel", Order = 2, ScalarStyle = ScalarStyle.DoubleQuoted)]
  public string DocPathRel { get; set; } = string.Empty;

  [YamlMember(Alias = "status", Order = 3)]
  public string Status { get; set; } = string.Empty;

  [YamlMember(Alias = "notes", Order = 4, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
  public string? Notes { get; set; }
}

public sealed class ToolRegistryDoc
{
  [YamlMember(Alias = "schema", Order = 1)]
  public string Schema { get; set; } = string.Empty;

  [YamlMember(Alias = "canonical_path", Order = 2)]
  public string CanonicalPath { get; set; } = string.Empty;

  [YamlMember(Alias = "tools", Order = 3)]
  public List<ToolRegistryEntry> Tools { get; set; } = new();

  [YamlMember(Alias = "generated_at_utc", Order = 4, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
  public string? GeneratedAtUtc { get; set; }

  [YamlMember(Alias = "placeholders", Order = 5)]
  public List<string> Placeholders { get; set; } = new();
}
