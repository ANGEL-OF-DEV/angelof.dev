// BootstrapToolsV0Models.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Text.Json.Serialization;

namespace Registry.Tool.Commands.BootstrapToolsV0;

public sealed record OperationResult(bool Ok, IReadOnlyList<string> Errors)
{
  public static OperationResult Success() => new(true, Array.Empty<string>());
  public static OperationResult Failure(params string[] errors) => new(false, errors);
  public static OperationResult Failure(List<string> errors) => new(false, errors);
}

public sealed record ToolCommandDoc(
  [property: JsonPropertyOrder(1)]
  [property: JsonPropertyName("name")]
  string Name,
  [property: JsonPropertyOrder(2)]
  [property: JsonPropertyName("example")]
  string Example
);

public sealed record ToolDoc(
  [property: JsonPropertyOrder(1)]
  [property: JsonPropertyName("schema")]
  string Schema,
  [property: JsonPropertyOrder(2)]
  [property: JsonPropertyName("id")]
  string Id,
  [property: JsonPropertyOrder(3)]
  [property: JsonPropertyName("canonical_path")]
  string CanonicalPath,
  [property: JsonPropertyOrder(4)]
  [property: JsonPropertyName("project_path_rel")]
  string ProjectPathRel,
  [property: JsonPropertyOrder(5)]
  [property: JsonPropertyName("kind")]
  string Kind,
  [property: JsonPropertyOrder(6)]
  [property: JsonPropertyName("commands")]
  IReadOnlyList<ToolCommandDoc> Commands,
  [property: JsonPropertyOrder(7)]
  [property: JsonPropertyName("generated_at_utc")]
  [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  string? GeneratedAtUtc,
  [property: JsonPropertyOrder(8)]
  [property: JsonPropertyName("placeholders")]
  IReadOnlyList<string> Placeholders
);

public sealed record ToolRegistryEntry(
  [property: JsonPropertyOrder(1)]
  [property: JsonPropertyName("id")]
  string Id,
  [property: JsonPropertyOrder(2)]
  [property: JsonPropertyName("doc_path_rel")]
  string DocPathRel,
  [property: JsonPropertyOrder(3)]
  [property: JsonPropertyName("status")]
  string Status,
  [property: JsonPropertyOrder(4)]
  [property: JsonPropertyName("notes")]
  [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  string? Notes
);

public sealed record ToolRegistryDoc(
  [property: JsonPropertyOrder(1)]
  [property: JsonPropertyName("schema")]
  string Schema,
  [property: JsonPropertyOrder(2)]
  [property: JsonPropertyName("canonical_path")]
  string CanonicalPath,
  [property: JsonPropertyOrder(3)]
  [property: JsonPropertyName("tools")]
  IReadOnlyList<ToolRegistryEntry> Tools,
  [property: JsonPropertyOrder(4)]
  [property: JsonPropertyName("generated_at_utc")]
  [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  string? GeneratedAtUtc,
  [property: JsonPropertyOrder(5)]
  [property: JsonPropertyName("placeholders")]
  IReadOnlyList<string> Placeholders
);
