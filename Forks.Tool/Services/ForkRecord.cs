// ForkRecord.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Forks.Tool.Services
{
  public record ForkRecord
  (
    [property: JsonPropertyName("package")] string Package,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("repo")] string Repo,
    [property: JsonPropertyName("motivation")] string? Motivation,
    [property: JsonPropertyName("notes")] string? Notes,
    [property: JsonPropertyName("tags")] IReadOnlyList<string>? Tags
  );
}
