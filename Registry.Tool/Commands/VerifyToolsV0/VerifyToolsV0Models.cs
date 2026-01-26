// VerifyToolsV0Models.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Registry.Tool.Commands.VerifyToolsV0;

public sealed record OperationResult(bool Ok, IReadOnlyList<string> Errors)
{
  public static OperationResult Success() => new(true, Array.Empty<string>());
  public static OperationResult Failure(List<string> errors) => new(false, errors);
}

public sealed record ToolDocParsed(
  string Schema,
  string Id,
  string CanonicalPath,
  string ProjectPathRel,
  string Kind,
  IReadOnlyList<ToolCommandParsed> Commands,
  IReadOnlyList<string> Placeholders
);

public sealed record ToolCommandParsed(string Name, string Example);

public sealed record RegistryParsed(
  string Schema,
  string CanonicalPath,
  IReadOnlyList<ToolRegistryEntryParsed> Tools,
  IReadOnlyList<string> Placeholders
);

public sealed record ToolRegistryEntryParsed(
  string Id,
  string DocPathRel,
  string Status,
  string? Notes
);
